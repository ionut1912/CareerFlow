using CareerFlow.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerFlow.Core.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Topic)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired(false);

        builder.HasMany(c => c.Chapters)
            .WithOne()
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.UserProfiles)
            .WithMany(up => up.Courses)
            .UsingEntity(j => j.ToTable("course_user_profiles"));

        builder.Navigation(c => c.Chapters)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(c => c.UserProfiles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
