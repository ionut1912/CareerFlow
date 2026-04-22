using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerFlow.Core.Infrastructure.Persistence.Configurations;

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

        builder.Property(a => a.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.ResetPasswordToken)
            .HasMaxLength(200);
        builder.HasIndex(a => a.Username).IsUnique();
        builder.HasIndex(a => a.Email).IsUnique();

        builder.Property(a => a.IsFounder)
            .HasDefaultValue(false)
            .IsRequired();
        builder.HasOne(a => a.UserProfile)
            .WithOne(up => up.Account)
            .HasForeignKey<UserProfile>(a => a.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}