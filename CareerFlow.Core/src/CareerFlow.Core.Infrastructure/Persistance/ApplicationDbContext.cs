using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Core.Infrastructure.Persistance;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<SystemDocument> SystemDocuments { get; set; }

    public DbSet<UserProfile> UserProfiles { get; set; }

    public DbSet<Course> Courses { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<SubChapter> SubChapters { get; set; }
    public DbSet<QuizQuestion> QuizQuestions { get; set; }
    public DbSet<CourseUpload> CourseUploads { get; set; }
    public DbSet<CourseJob> CourseJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}