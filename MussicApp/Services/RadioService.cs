using Microsoft.EntityFrameworkCore;
using MussicApp.Data;
using MussicApp.Models;

namespace MussicApp.Services
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
                    .ThenInclude(tg => tg.Genre)
                .FirstOrDefaultAsync(t =>
                    t.Id == seedTrackId &&
                    t.Status == TrackStatus.Approved);

            if (seed == null)
                return new();

            // ===== ІСТОРІЯ КОРИСТУВАЧА =====
            var listenedTrackIds = await _db.UserListeningHistories
    .Where(h => h.UserId == userId)
    .Select(h => h.TrackId)
    .ToHashSetAsync();

            var hasHistory = listenedTrackIds.Any();

            Dictionary<Guid, int> userGenreWeights = new();

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

            var seedGenreIds = seed.TrackGenres
                .Select(g => g.GenreId)
                .ToHashSet();

            var candidates = await _db.Tracks
                .Include(t => t.Artist)
                .Include(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
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

                // ❌ Уже слухав
                if (listenedTrackIds.Contains(track.Id))
                {
                    score -= 30;
                    reasons.Add("Already listened");
                }

                // 🎼 Спільні жанри з seed
                var commonGenres = track.TrackGenres
                    .Count(g => seedGenreIds.Contains(g.GenreId));

                if (commonGenres > 0)
                {
                    score += commonGenres * 40;
                    reasons.Add("Similar to seed genres");
                }

                // 👤 Жанрові вподобання користувача
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

                // 🎤 Той самий артист
                if (track.ArtistId == seed.ArtistId)
                {
                    score += 50;
                    reasons.Add("Same artist");
                }

                // ❤️ Популярність
                score += track.LikedByUsers.Count * 2;

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
                // 🎧 Історія прослуховувань
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
                // 🧊 Cold start — favorite genres
                genreWeights = await _db.UserFavoriteGenres
                    .Where(f => f.UserId == userId)
                    .GroupBy(f => f.GenreId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => 5
                    );
            }

            if (!genreWeights.Any())
                return new();

            var candidates = await _db.Tracks
                .Include(t => t.Artist)
                .Include(t => t.TrackGenres)
                    .ThenInclude(tg => tg.Genre)
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

                score += track.LikedByUsers.Count * 2;

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
