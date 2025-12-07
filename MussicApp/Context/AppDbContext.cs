using Microsoft.EntityFrameworkCore;
using MussicApp.Models;


public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<Track> Tracks { get; set; }
    public DbSet<User> Users { get; set; }
}