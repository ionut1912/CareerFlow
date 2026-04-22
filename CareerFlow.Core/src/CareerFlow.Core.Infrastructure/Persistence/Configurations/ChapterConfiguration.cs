using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerFlow.Core.Infrastructure.Persistence.Configurations;

public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
{
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.CoreConcept)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired(false);

        builder.HasMany(c => c.SubChapters)
            .WithOne(s => s.Chapter)
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.SubChapters)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasOne(c => c.Course)
            .WithMany(c => c.Chapters)
            .HasForeignKey(f => f.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}