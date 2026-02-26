using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerFlow.Core.Infrastructure.Persistance.Configurations;

public class SystemDocumentsConfiguration : IEntityTypeConfiguration<SystemDocument>
{
    public void Configure(EntityTypeBuilder<SystemDocument> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.DocumentType)
            .IsUnique();

        builder.ToTable("SystemDocuments");
    }
}