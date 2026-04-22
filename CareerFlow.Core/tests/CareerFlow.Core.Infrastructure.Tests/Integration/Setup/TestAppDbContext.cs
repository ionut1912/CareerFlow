using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Core.Infrastructure.Tests.Integration.Setup;

public sealed class TestAppDbContext(DbContextOptions<TestAppDbContext> options) : DbContext(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<SubChapter> SubChapters => Set<SubChapter>();
    public DbSet<CourseJob> CourseJobs => Set<CourseJob>();
    public DbSet<CourseUpload> CourseUploads => Set<CourseUpload>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<SystemDocument> SystemDocuments => Set<SystemDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new UserProfileConfiguration());
        modelBuilder.ApplyConfiguration(new ChapterConfiguration());
        modelBuilder.ApplyConfiguration(new CourseConfiguration());
        modelBuilder.ApplyConfiguration(new SubChapterConfiguration());
        modelBuilder.ApplyConfiguration(new CourseJobConfiguration());
        modelBuilder.ApplyConfiguration(new CourseUploadConfiguration());
        modelBuilder.ApplyConfiguration(new QuizQuestionConfiguration());
        modelBuilder.ApplyConfiguration(new SystemDocumentsConfiguration());
    }
}