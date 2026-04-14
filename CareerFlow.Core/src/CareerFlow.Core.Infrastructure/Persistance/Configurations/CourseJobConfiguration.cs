using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerFlow.Core.Infrastructure.Persistance.Configurations;

public sealed class CourseJobConfiguration : IEntityTypeConfiguration<CourseJob>
{
    public void Configure(EntityTypeBuilder<CourseJob> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.UploadId)
            .IsRequired();

        builder.Property(x => x.CourseId);

        builder.Property(cj => cj.Status)
            .HasConversion(
                lt => lt.Value,
                value => JobStatus.FromString(value))
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("learning_type");

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);
    }
}