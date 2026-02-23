using Microsoft.EntityFrameworkCore;
using MussicApp.Data;
using MussicApp.Models.TracksRelated;

namespace MussicApp.Services.Radio
{
    public class RadioService : IRadioService
    {
        private readonly AppDbContext _db;

        public RadioService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<RadioQueueItemDto>> BuildRadioQueueAsync(
    Guid seedTrackId,
    Guid userId,
    int limit)
        {
            var seed = await _db.Tracks
                .Include(t => t.Artist)
                .Include(t => t.TrackGenres)
                .FirstOrDefaultAsync(t =>
                    t.Id == seedTrackId &&
                    t.Status == TrackStatus.Approved);

            if (seed == null)
                return new();

            var listenedTrackIds = await _db.UserListeningHistories
                .Where(h => h.UserId == userId)
                .Select(h => h.TrackId)
                .ToHashSetAsync();

            var hasHistory = listenedTrackIds.Any();

            // 1️⃣ User genre profile
            Dictionary<Guid, int> userGenreWeights;

            if (hasHistory)
            {
                userGenreWeights = await _db.UserListeningHistories
                    .Where(h => h.UserId == userId)
                    .Join(_db.TrackGenres,
                        h => h.TrackId,
                        tg => tg.TrackId,
                        (_, tg) => tg.GenreId)
                    .GroupBy(g => g)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            else
            {
                userGenreWeights = await _db.UserFavoriteGenres
                    .Where(f => f.UserId == userId)
                    .ToDictionaryAsync(f => f.GenreId, _ => 5);
            }

            var userGenreIds = userGenreWeights.Keys.ToHashSet();
            var seedGenreIds = seed.TrackGenres.Select(g => g.GenreId).ToHashSet();

            // 2️⃣ Find similar users
            var similarUserIds = await _db.UserFavoriteGenres
                .Where(f =>
                    f.UserId != userId &&
                    userGenreIds.Contains(f.GenreId))
                .GroupBy(f => f.UserId)
                .Where(g => g.Count() >= 2)
                .Select(g => g.Key)
                .Take(50)
                .ToListAsync();

            // 3️⃣ Collaborative scores
            var collaborativeScores = await _db.UserListeningHistories
                .Where(h => similarUserIds.Contains(h.UserId))
                .GroupBy(h => h.TrackId)
                .Select(g => new
                {
                    TrackId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.TrackId, x => x.Count);

            var candidates = await _db.Tracks
                .Include(t => t.Artist)
                .Include(t => t.TrackGenres)
                .Include(t => t.LikedByUsers)
                .Where(t =>
                    t.Status == TrackStatus.Approved &&
                    t.Id != seed.Id)
                .ToListAsync();

            var scored = new List<(Track track, double score, List<string> reasons)>();

            foreach (var track in candidates)
            {
                double score = 0;
                var reasons = new List<string>();

                // Already listened penalty
                if (listenedTrackIds.Contains(track.Id))
                {
                    score -= 30;
                    reasons.Add("Already listened");
                }

                // Seed genre similarity
                var commonGenres = track.TrackGenres
                    .Count(g => seedGenreIds.Contains(g.GenreId));

                if (commonGenres > 0)
                {
                    score += commonGenres * 40;
                    reasons.Add("Similar to seed genres");
                }

                // User genre affinity
                foreach (var tg in track.TrackGenres)
                {
                    if (userGenreWeights.TryGetValue(tg.GenreId, out var weight))
                    {
                        score += weight * 10;
                        reasons.Add(hasHistory
                            ? "Based on listening history"
                            : "Based on favorite genres");
                    }
                }

                // Same artist boost
                if (track.ArtistId == seed.ArtistId)
                {
                    score += 20;
                    reasons.Add("Same artist");
                }

                // Popularity
                score += track.LikedByUsers.Count * 2;

                // Collaborative boost
                if (collaborativeScores.TryGetValue(track.Id, out var collabCount))
                {
                    var collabBoost = Math.Log(collabCount + 1) * 8;
                    score += collabBoost;
                    reasons.Add("Popular among similar listeners");
                }

                if (score > 0)
                    scored.Add((track, score, reasons));
            }

            return scored
                .OrderByDescending(x => x.score)
                .Take(limit)
                .Select(x => new RadioQueueItemDto
                {
                    TrackId = x.track.Id,
                    Title = x.track.Title,
                    ArtistName = x.track.Artist!.Name,
                    Score = x.score,
                    Reasons = x.reasons.Distinct().ToList()
                })
                .ToList();
        }


        public async Task<List<RadioQueueItemDto>> BuildRecommendationsAsync(
    Guid userId,
    int limit)
        {
            var listenedTrackIds = await _db.UserListeningHistories
                .Where(h => h.UserId == userId)
                .Select(h => h.TrackId)
                .ToHashSetAsync();

            var hasHistory = listenedTrackIds.Any();

            Dictionary<Guid, int> genreWeights;

            if (hasHistory)
            {
                genreWeights = await _db.UserListeningHistories
                    .Where(h => h.UserId == userId)
                    .Join(_db.TrackGenres,
                        h => h.TrackId,
                        tg => tg.TrackId,
                        (_, tg) => tg.GenreId)
                    .GroupBy(g => g)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());
            }
            else
            {
                genreWeights = await _db.UserFavoriteGenres
                    .Where(f => f.UserId == userId)
                    .ToDictionaryAsync(f => f.GenreId, _ => 5);
            }

            if (!genreWeights.Any())
                return new();

            var userGenreIds = genreWeights.Keys.ToHashSet();

            // Similar users
            var similarUserIds = await _db.UserFavoriteGenres
                .Where(f =>
                    f.UserId != userId &&
                    userGenreIds.Contains(f.GenreId))
                .GroupBy(f => f.UserId)
                .Where(g => g.Count() >= 2)
                .Select(g => g.Key)
                .Take(50)
                .ToListAsync();

            // Collaborative scores
            var collaborativeScores = await _db.UserListeningHistories
                .Where(h => similarUserIds.Contains(h.UserId))
                .GroupBy(h => h.TrackId)
                .Select(g => new
                {
                    TrackId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.TrackId, x => x.Count);

            var candidates = await _db.Tracks
                .Include(t => t.Artist)
                .Include(t => t.TrackGenres)
                .Include(t => t.LikedByUsers)
                .Where(t =>
                    t.Status == TrackStatus.Approved &&
                    !listenedTrackIds.Contains(t.Id))
                .ToListAsync();

            var scored = new List<(Track track, double score, List<string> reasons)>();

            foreach (var track in candidates)
            {
                double score = 0;
                var reasons = new List<string>();

                // Genre affinity
                foreach (var tg in track.TrackGenres)
                {
                    if (genreWeights.TryGetValue(tg.GenreId, out var weight))
                    {
                        score += weight * 15;
                        reasons.Add(hasHistory
                            ? "Based on listening history"
                            : "Based on favorite genres");
                    }
                }

                // Popularity
                score += track.LikedByUsers.Count * 2;

                // Collaborative
                if (collaborativeScores.TryGetValue(track.Id, out var collabCount))
                {
                    var collabBoost = Math.Log(collabCount + 1) * 8;
                    score += collabBoost;
                    reasons.Add("Popular among similar listeners");
                }

                if (score > 0)
                    scored.Add((track, score, reasons));
            }

            return scored
                .OrderByDescending(x => x.score)
                .Take(limit)
                .Select(x => new RadioQueueItemDto
                {
                    TrackId = x.track.Id,
                    Title = x.track.Title,
                    ArtistName = x.track.Artist!.Name,
                    Score = x.score,
                    Reasons = x.reasons.Distinct().ToList()
                })
                .ToList();
        }


        public async Task<List<ListeningHistoryItemDto>> GetListeningHistoryAsync(Guid userId, int limit)
        {
            return await _db.UserListeningHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.PlayedAt)
                .Include(h => h.Track)
                    .ThenInclude(t => t.Artist)
                .Take(limit)
                .Select(h => new ListeningHistoryItemDto
                {
                    TrackId = h.TrackId,
                    Title = h.Track.Title,
                    ArtistName = h.Track.Artist!.Name,
                    PlayedAt = h.PlayedAt,
                    PlayedSeconds = h.PlayedDuration.TotalSeconds
                })
                .ToListAsync();
        }

        public async Task<List<RecentlyPlayedDto>> GetRandomRecentlyPlayedAsync(Guid userId, int sourceLimit, int resultLimit)
        {
            var history = await _db.UserListeningHistories
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.PlayedAt)
                .Take(sourceLimit)
                .Include(h => h.Track)
                    .ThenInclude(t => t.Artist)
                .ToListAsync();

            var distinctTracks = history
                .GroupBy(h => h.TrackId)
                .Select(g => g.First())
                .ToList();

            var random = new Random();

            var selected = distinctTracks
                .OrderBy(_ => random.Next())
                .Take(resultLimit)
                .Select(h => new RecentlyPlayedDto
                {
                    TrackId = h.TrackId,
                    Title = h.Track.Title,
                    ArtistName = h.Track.Artist!.Name,
                    LastPlayedAt = h.PlayedAt
                })
                .ToList();

            return selected;
        }


    }
}
