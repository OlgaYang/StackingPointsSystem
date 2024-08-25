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
        modelBuilder.Entity<Asset>().ToTable("Assets").HasKey(x => new { x.Username, x.CreatedTime });
    }

    public DbSet<UserEntity> UserEntities { get; set; }
    public DbSet<Asset> Assets { get; set; }
}

public class UserEntity
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
}