using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerFlow.Core.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(u => u.Id);

        builder.HasOne(up => up.Account)
            .WithOne(a => a.UserProfile)
            .HasForeignKey<UserProfile>(up => up.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

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

        builder.Property(up => up.LearningType)
            .HasConversion(
                lt => lt.Value,
                value => LearningType.FromString(value))
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("learning_type");

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
        builder.HasMany(up => up.Courses)
            .WithMany(c => c.UserProfiles)
            .UsingEntity(j => j.ToTable("course_user_profiles"));

        builder.Navigation(up => up.Courses)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Property<List<string>>("_finishedChapters")
            .HasColumnName("finished_chapters")
            .HasColumnType("jsonb")
            .IsRequired();
    }
}
