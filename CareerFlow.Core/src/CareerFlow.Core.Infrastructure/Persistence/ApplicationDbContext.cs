using CareerFlow.Core.Domain.Entities;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Core.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    [UsedImplicitly]
    public DbSet<Account> Accounts { get; set; }
    [UsedImplicitly]
    public DbSet<RefreshToken> RefreshTokens { get; set; }
   [UsedImplicitly]
    public DbSet<SystemDocument> SystemDocuments { get; set; }
    [UsedImplicitly]
    public DbSet<UserProfile> UserProfiles { get; set; }
    [UsedImplicitly]
    public DbSet<Course> Courses { get; set; }
    [UsedImplicitly]
    public DbSet<Chapter> Chapters { get; set; }
    [UsedImplicitly]
    public DbSet<SubChapter> SubChapters { get; set; }
    [UsedImplicitly]
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    [UsedImplicitly]
    public DbSet<CourseUpload> CourseUploads { get; set; }
    [UsedImplicitly]
    public DbSet<CourseJob> CourseJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}