using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarrerFlow.Core.Infrastructure.Persistance.Configurations;

public class LegalDocConfiguration : IEntityTypeConfiguration<LegalDoc>
{
    public void Configure(EntityTypeBuilder<LegalDoc> builder)
    {
        builder.OwnsOne(p => p.Type, type =>
        {
            type.Property(c => c.Value)
                .HasColumnName("Type")
                .HasMaxLength(20)
                .IsRequired();
        });

    }
}
