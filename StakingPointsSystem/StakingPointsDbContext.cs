using Microsoft.EntityFrameworkCore;

namespace StakingPointsSystem;

public class StakingPointsDbContext : DbContext
{
    public StakingPointsDbContext (DbContextOptions<StakingPointsDbContext> options)
        : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>().ToTable("Users").HasKey(x=>x.UserName);
    }
    
    public DbSet<UserEntity> UserEntities { get; set; } 
}

public class UserEntity
{
    public string  UserName { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
}