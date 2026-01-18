using Microsoft.EntityFrameworkCore;
using MussicApp.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MussicApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<AlbumTrack> AlbumTracks => Set<AlbumTrack>();
    public DbSet<UserLikedTrack> UserLikedTracks => Set<UserLikedTrack>();
    public DbSet<Artist> Artists { get; set; } = null!;
    public DbSet<Comments> Comments { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ================================
        // UserLikedTrack (many-to-many)
        // ================================
        modelBuilder.Entity<UserLikedTrack>()
            .HasKey(ul => new { ul.UserId, ul.TrackId });

        modelBuilder.Entity<UserLikedTrack>()
            .HasOne(ul => ul.User)
            .WithMany(u => u.LikedTracks)
            .HasForeignKey(ul => ul.UserId);

        modelBuilder.Entity<UserLikedTrack>()
            .HasOne(ul => ul.Track)
            .WithMany(t => t.LikedByUsers)
            .HasForeignKey(ul => ul.TrackId);

        // ================================
        // AlbumTrack (many-to-many)
        // ================================
        modelBuilder.Entity<AlbumTrack>()
            .HasKey(at => new { at.AlbumId, at.TrackId });

        modelBuilder.Entity<AlbumTrack>()
            .HasOne(at => at.Album)
            .WithMany(a => a.AlbumTracks)
            .HasForeignKey(at => at.AlbumId);

        modelBuilder.Entity<AlbumTrack>()
            .HasOne(at => at.Track)
            .WithMany()
            .HasForeignKey(at => at.TrackId);

        // ================================
        // User
        // ================================
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // ================================
        // Track
        // ================================
        modelBuilder.Entity<Track>()
            .Property(t => t.Duration)
            .HasConversion(
                v => v.Ticks,
                v => TimeSpan.FromTicks(v)
            );




        // ================================
        // Album
        // ================================
        modelBuilder.Entity<Album>()
            .HasMany(a => a.AlbumTracks)
            .WithOne(at => at.Album)
            .HasForeignKey(at => at.AlbumId);


        // ================================
        // Comments
        // ================================

        modelBuilder.Entity<Comments>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
