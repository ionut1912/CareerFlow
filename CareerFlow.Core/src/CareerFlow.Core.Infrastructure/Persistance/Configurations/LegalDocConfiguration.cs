using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerFlow.Core.Infrastructure.Persistance.Configurations;

public class LegalDocConfiguration : IEntityTypeConfiguration<LegalDoc>
{
    public void Configure(EntityTypeBuilder<LegalDoc> builder)
    {
        builder.Property(l => l.Type)
            .HasColumnName("Type")
            .HasMaxLength(20)
            .IsRequired()
            .HasConversion(
                v => v.Value,
                v => LegalDocType.FromString(v)
            );
    }
}