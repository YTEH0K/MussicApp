using MongoDB.Driver;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MussicApp.Models;
using MussicApp.Data;
using MongoDB.Driver.GridFS;
using Microsoft.EntityFrameworkCore;

namespace MussicApp.Services;

public class TrackService : ITrackService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _gridFS;

    public TrackService(
        AppDbContext db,
        IMongoClient mongo,
        IFileStorageService gridFS,
        IOptions<MongoDbSettings> options)
    {
        _db = db;
        _gridFS = gridFS;
    }

    public async Task<Track> CreateAsync(
    IFormFile audio,
    IFormFile cover,
    string title,
    string lyrics,
    Guid artistId,
    Guid? albumId,
    Guid ownerId,
    IEnumerable<Guid>? genreIds = null)
    {
        if (audio == null || audio.Length == 0)
            throw new ArgumentException("Audio file is required");

        if (cover == null || cover.Length == 0)
            throw new ArgumentException("Cover image is required");

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required");

        // 🔒 Перевірка артиста (FK safety)
        var artistExists = await _db.Artists.AnyAsync(a => a.Id == artistId);
        if (!artistExists)
            throw new InvalidOperationException(
                $"Artist with id {artistId} does not exist"
            );

        ObjectId audioId = ObjectId.Empty;
        ObjectId coverId = ObjectId.Empty;

        try
        {
            // 🎵 upload audio
            await using (var audioStream = audio.OpenReadStream())
            {
                audioId = await _gridFS.UploadAsync(
                    audioStream,
                    audio.FileName,
                    audio.ContentType
                );
            }

            // 🖼 upload cover
            await using (var coverStream = cover.OpenReadStream())
            {
                coverId = await _gridFS.UploadAsync(
                    coverStream,
                    cover.FileName,
                    cover.ContentType
                );
            }

            var track = new Track
            {
                Id = Guid.NewGuid(),
                Title = title,
                ArtistId = artistId,
                Lyrics = lyrics,
                AlbumId = albumId,
                OwnerId = ownerId,
                FileId = audioId.ToString(),
                CoverFileId = coverId.ToString(),
                UploadedAt = DateTime.UtcNow,
                TrackGenres = (genreIds ?? Enumerable.Empty<Guid>())
                    .Distinct()
                    .Select(gid => new TrackGenre { TrackId = Guid.Empty, GenreId = gid })
                    .ToList()
            };

            foreach (var tg in track.TrackGenres)
                tg.TrackId = track.Id;

            _db.Tracks.Add(track);
            await _db.SaveChangesAsync();

            return track;
        }
        catch
        {
            if (audioId != ObjectId.Empty)
                await _gridFS.DeleteAsync(audioId);

            if (coverId != ObjectId.Empty)
                await _gridFS.DeleteAsync(coverId);

            throw;
        }
    }

    public async Task<Track> UpdateAsync(Track track)
    {
        _db.Tracks.Update(track);
        await _db.SaveChangesAsync();
        return track;
    }

    public async Task DeleteAsync(Track track)
    {
        await _gridFS.DeleteAsync(ObjectId.Parse(track.FileId));

        if (!string.IsNullOrEmpty(track.CoverFileId))
            await _gridFS.DeleteAsync(ObjectId.Parse(track.CoverFileId));

        _db.Tracks.Remove(track);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Track>> GetByOwnerIdAsync(Guid ownerId)
    {
        return await _db.Tracks
            .Where(t => t.OwnerId == ownerId)
            .Include(t => t.TrackGenres)
                .ThenInclude(tg => tg.Genre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Track>> GetAllAsync()
    {
        return await _db.Tracks
           .Where(t => t.Status == TrackStatus.Approved)
           .Include(t => t.Artist)
           .Include(t => t.Album)
           .Include(t => t.TrackGenres)
               .ThenInclude(tg => tg.Genre)
           .OrderByDescending(t => t.UploadedAt)
           .ToListAsync();
    }


    public async Task<Track?> GetByIdAsync(Guid id)
    {
        return await _db.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.TrackGenres)
                .ThenInclude(tg => tg.Genre)
            .FirstOrDefaultAsync(t => t.Id == id);
    }


    public async Task<IEnumerable<Artist>> GetAllArtistsAsync()
    {
        return await _db.Artists.ToListAsync();
    }

    public async Task<Artist?> GetArtistByIdAsync(Guid id)
    {
        return await _db.Artists.FindAsync(id);
    }

    public async Task<IEnumerable<Track>> GetByGenreSlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return Enumerable.Empty<Track>();

        slug = slug.Trim().ToLowerInvariant();

        return await _db.Tracks
            .Include(t => t.Artist)
            .Include(t => t.Album)
            .Include(t => t.TrackGenres)
                .ThenInclude(tg => tg.Genre)
            .Where(t => t.Status == TrackStatus.Approved
                        && t.TrackGenres.Any(tg => tg.Genre != null
                                                   && tg.Genre.Slug.ToLower() == slug))
            .OrderByDescending(t => t.UploadedAt)
            .ToListAsync();
    }

    public async Task AddListeningHistoryAsync(
    Guid userId,
    Guid trackId,
    TimeSpan playedDuration)
    {
        // базовая защита от мусора
        if (playedDuration.TotalSeconds < 5)
            return;

        var exists = await _db.Tracks
            .AnyAsync(t => t.Id == trackId);

        if (!exists)
            throw new InvalidOperationException("Track not found");

        var history = new UserListeningHistory
        {
            UserId = userId,
            TrackId = trackId,
            PlayedAt = DateTime.UtcNow,
            PlayedDuration = playedDuration
        };

        _db.UserListeningHistories.Add(history);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserListeningHistory>> GetListeningHistoryAsync(
    Guid userId,
    int limit = 50)
    {
        return await _db.UserListeningHistories
            .Where(h => h.UserId == userId)
            .Include(h => h.Track)
                .ThenInclude(t => t.Artist)
            .OrderByDescending(h => h.PlayedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task SetUserFavoriteGenresAsync(
    Guid userId,
    IEnumerable<Guid> genreIds)
    {
        var ids = genreIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            throw new InvalidOperationException("Select at least one genre");

        var validGenreIds = await _db.Genres
            .Where(g => ids.Contains(g.Id) && g.IsActive)
            .Select(g => g.Id)
            .ToListAsync();

        if (validGenreIds.Count == 0)
            throw new InvalidOperationException("Invalid genres");

        var alreadySelected = await _db.UserFavoriteGenres
            .AnyAsync(x => x.UserId == userId);

        if (alreadySelected)
            throw new InvalidOperationException(
                "Favorite genres already selected"
            );

        var entities = validGenreIds.Select(gid =>
            new UserFavoriteGenre
            {
                UserId = userId,
                GenreId = gid
            });

        _db.UserFavoriteGenres.AddRange(entities);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Genre>> GetAllGenresAsync()
    {
        return await _db.Genres
            .Where(g => g.IsActive)
            .OrderBy(g => g.Name)
            .ToListAsync();
    }


}
