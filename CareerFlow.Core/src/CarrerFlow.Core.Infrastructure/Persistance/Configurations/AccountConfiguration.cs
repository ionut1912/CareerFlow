using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerFlow.Core.Infrastructure.Persistance.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Password)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Username)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(a => a.Username).IsUnique();

        builder.Property(a => a.IsFounder)
            .HasDefaultValue(false)
            .IsRequired();
    }
}