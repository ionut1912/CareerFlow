using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistance.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Core.Infrastructure.Test.Integration.Setup;

/// <summary>
public sealed class TestAppDbContext(DbContextOptions<TestAppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Account ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Account>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Email).IsRequired().HasMaxLength(256);
            b.Property(a => a.Username).IsRequired().HasMaxLength(100);
            b.HasIndex(a => a.Email).IsUnique();
        });

        // ── RefreshToken ─────────────────────────────────────────────────────
        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.JwtToken).IsRequired();
            b.Property(r => r.Token).IsRequired();
            b.HasOne<Account>()
                .WithMany()
                .HasForeignKey(r => r.UserId);
        });

        // ── UserProfile – reuse the real EF configuration ────────────────────
        modelBuilder.ApplyConfiguration(new UserProfileConfiguration());

        modelBuilder.Entity<UserProfile>()
            .HasOne(up => up.Account)
            .WithMany()
            .HasForeignKey(up => up.AccountId);
    }
}