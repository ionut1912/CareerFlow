using CareerFlow.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerFlow.Core.Infrastructure.Persistence.Configurations;

public class SubChapterConfiguration : IEntityTypeConfiguration<SubChapter>
{
    public void Configure(EntityTypeBuilder<SubChapter> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Summary)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.TheoryHtml)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired(false);
    }
}
