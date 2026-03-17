using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerFlow.Core.Infrastructure.Persistance.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.AccountId)
            .IsRequired();
        builder.HasIndex(up => up.AccountId)
            .IsUnique();
        builder.Property(up => up.Domain)
            .HasMaxLength(100);

        builder.Property(up => up.CorrectAnswersForQuiz)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(up => up.IncorrectAnswersForQuiz)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(up => up.Experience)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(up => up.CreatedAt).IsRequired();
        builder.Property(up => up.UpdatedAt).IsRequired(false);

        // LearningType value object → single string column
        builder.Property(up => up.LearningType)
            .HasConversion(
                lt => lt.Value,
                value => LearningType.FromString(value))
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("learning_type");

        // UserTypes value objects → owned collection table
        builder.OwnsMany(up => up.UserTypes, ut =>
            {
                ut.ToTable("user_profile_user_types");

                ut.WithOwner().HasForeignKey("UserProfileId");

                ut.Property(u => u.Value)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("user_type");

                ut.HasKey("UserProfileId", "Value");
            })
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}