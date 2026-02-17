using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Core.Infrastructure.Test.Integration.Setup;

public sealed class TestAppDbContext(DbContextOptions<TestAppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<LegalDoc> LegalDocs => Set<LegalDoc>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Email).IsRequired().HasMaxLength(256);
            b.Property(a => a.Username).IsRequired().HasMaxLength(100);
            b.HasIndex(a => a.Email).IsUnique();
        });

        modelBuilder.Entity<LegalDoc>(b =>
        {
            b.HasKey(l => l.Id);
            b.Property(l => l.Type)
                .HasConversion(v => v.Value, v => LegalDocType.FromString(v))
                .IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.JwtToken).IsRequired();
            b.Property(r => r.Token).IsRequired();
            b.HasOne<Account>()
                .WithMany()
                .HasForeignKey(r => r.UserId);
        });
    }
}
