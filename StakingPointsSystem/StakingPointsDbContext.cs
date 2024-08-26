using Microsoft.EntityFrameworkCore;
using StakingPointsSystem.Models;

namespace StakingPointsSystem;

public class StakingPointsDbContext : DbContext
{
    public StakingPointsDbContext()
    {
    }

    public StakingPointsDbContext(DbContextOptions<StakingPointsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>().ToTable("Users").HasKey(x => x.Username);
        modelBuilder.Entity<Asset>().ToTable("Assets").HasKey(x => new { x.UserId, x.CreatedTime });
        modelBuilder.Entity<UserScore>().ToTable("UserScores").HasKey(x => x.UserId);
    }

    public DbSet<UserEntity> UserEntities { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<UserScore> UserScores { get; set; }
}

public class UserScore
{
    public int UserId { get; set; }
    public decimal TotalScore { get; set; }
    public DateTime LastUpdatedTime { get; set; }
}

public class UserEntity
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
}