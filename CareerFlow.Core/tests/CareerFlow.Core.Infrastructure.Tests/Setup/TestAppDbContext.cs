using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistance.Configurations;
using Microsoft.EntityFrameworkCore;

<<<<<<< HEAD
namespace CareerFlow.Core.Infrastructure.Test.Setup;
=======
namespace CareerFlow.Core.Infrastructure.Tests.Setup;
>>>>>>> master

public sealed class TestAppDbContext(DbContextOptions<TestAppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
    }
}