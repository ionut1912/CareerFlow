using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerFlow.Core.Infrastructure.Persistence.Configurations;

public class SystemDocumentsConfiguration : IEntityTypeConfiguration<SystemDocument>
{
    public void Configure(EntityTypeBuilder<SystemDocument> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.DocumentType)
            .IsUnique();

        builder.Property(x => x.DocumentType)
            .HasMaxLength(100);

        builder.Property(x => x.CurrentETag)
            .HasMaxLength(100);

        builder.ToTable("SystemDocuments");
    }
}