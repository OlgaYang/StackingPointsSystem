using Microsoft.EntityFrameworkCore;
using StakingPointsSystem.Models;
using StakingPointsSystem.Services;

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
        modelBuilder.Entity<User>().ToTable("Users").HasKey(x => x.UserId);
        modelBuilder.Entity<Asset>().ToTable("Assets").HasKey(x => new { x.UserId, x.AssetType });
        modelBuilder.Entity<UserScore>().ToTable("UserScores").HasKey(x => x.UserId);
        modelBuilder.Entity<Balance>().ToTable("Balances").HasKey(x => new { x.UserId, x.AssetType });
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<UserScore> UserScores { get; set; }
    public DbSet<Balance> Balances { get; set; }
}

public class Balance
{
    public int UserId { get; set; }
    public AssetType AssetType { get; set; }
    public decimal Unit { get; set; }

    public IStatement ToStatement()
    {
        return new BalanceStatement(UserId, AssetType, Unit);
    }
}

public class UserScore
{
    public int UserId { get; set; }
    public decimal TotalScore { get; set; }
    public DateTime LastUpdatedTime { get; set; }
}

public class User
{
    public int UserId { get; set; }
    // public string Username { get; set; }
    // public string Email { get; set; }
    public string Name { get; set; }
    // public string Password { get; set; }
}