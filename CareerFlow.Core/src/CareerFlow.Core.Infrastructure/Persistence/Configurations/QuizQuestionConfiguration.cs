using CareerFlow.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareerFlow.Core.Infrastructure.Persistence.Configurations;

public sealed class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Question)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Options)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CorrectAnswer)
            .HasMaxLength(10000)
            .IsRequired();


        builder.HasIndex(x => x.ChapterId)
            .HasDatabaseName("ix_quiz_questions_chapter_id");

        builder.HasIndex(x => x.SubChapterId)
            .HasDatabaseName("ix_quiz_questions_sub_chapter_id");
    }
}
