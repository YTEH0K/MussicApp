using Microsoft.EntityFrameworkCore;
using MussicApp.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Track> Tracks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Album> Albums { get; set; }
    public DbSet<AlbumTrack> AlbumTracks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AlbumTrack>()
            .HasKey(at => new { at.AlbumId, at.TrackId });

        modelBuilder.Entity<AlbumTrack>()
            .HasOne(at => at.Album)
            .WithMany(a => a.AlbumTracks)
            .HasForeignKey(at => at.AlbumId);

        modelBuilder.Entity<AlbumTrack>()
            .HasOne(at => at.Track)
            .WithMany(t => t.AlbumTracks)
            .HasForeignKey(at => at.TrackId);
    }
}
