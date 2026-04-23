using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistence.Repositories;
using CareerFlow.Core.Infrastructure.Tests.Integration.Setup;

using Shouldly;

using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class CourseRepositoryTests : BaseRepositoryTest, IAsyncLifetime
{
    private readonly CourseRepository _sut;

    public CourseRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new CourseRepository(Context.Courses);
    }

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Context.SubChapters.RemoveRange(Context.SubChapters);
        Context.Chapters.RemoveRange(Context.Chapters);
        Context.Courses.RemoveRange(Context.Courses);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task AddAsync_Course_PersistsToDatabase()
    {
        // Arrange
        Course course = CreateCourse("Clean Architecture");

        // Act
        await _sut.AddAsync(course, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        Course? saved = await Context.Courses.FindAsync(course.Id);
        saved.ShouldNotBeNull();
        saved.Topic.ShouldBe("Clean Architecture");
    }

    [Fact]
    public async Task AddAsync_Course_PersistsCascadedChaptersAndSubChapters()
    {
        // Arrange
        Course course = CreateCourse("DDD", [
            CreateChapter(1, "Aggregates", 2),
            CreateChapter(2, "Value Objects", 3)
        ]);

        // Act
        await _sut.AddAsync(course, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        int chaptersCount = Context.Chapters.Count(c => c.CourseId == course.Id);
        int subChaptersCount = Context.SubChapters.Count();
        chaptersCount.ShouldBe(2);
        subChaptersCount.ShouldBe(5);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCourse()
    {
        // Arrange
        Course course = await SeedCourseAsync();

        // Act
        Course? result = await _sut.GetByIdAsync(course.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(course.Id);
        result.Topic.ShouldBe(course.Topic);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Act
        Course? result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithChaptersInclude_ReturnsChapters()
    {
        // Arrange
        Course course = await SeedCourseAsync(chapters:
        [
            CreateChapter(1, "Chapter 1"),
            CreateChapter(2, "Chapter 2")
        ]);

        // Act
        Course? result = await _sut.GetByIdAsync(course.Id, CancellationToken.None, c => c.Chapters);

        // Assert
        result.ShouldNotBeNull();
        result.Chapters.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllAsync_MultipleCourses_ReturnsAll()
    {
        // Arrange
        await SeedCourseAsync("C# Basics");
        await SeedCourseAsync("ASP.NET Core");

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(c => c.Topic == "C# Basics");
        result.ShouldContain(c => c.Topic == "ASP.NET Core");
    }

    [Fact]
    public async Task GetAllAsync_WithChaptersInclude_ReturnsCoursesWithChapters()
    {
        // Arrange
        await SeedCourseAsync(chapters:
        [
            CreateChapter(1, "Chapter 1"),
            CreateChapter(2, "Chapter 2")
        ]);

        // Act
        var result = (await _sut.GetAllAsync(CancellationToken.None, c => c.Chapters)).ToList();

        // Assert
        result.ShouldAllBe(c => c.Chapters.Any());
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsEmptyCollection()
    {
        // Act
        IEnumerable<Course> result = await _sut.GetAllAsync();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Delete_ExistingCourse_RemovesFromDatabase()
    {
        // Arrange
        Course course = await SeedCourseAsync();
        Course? tracked = await Context.Courses.FindAsync(course.Id);

        // Act
        _sut.Delete(tracked!);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        Course? deleted = await Context.Courses.FindAsync(course.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Delete_ExistingCourse_CascadesChaptersAndSubChapters()
    {
        // Arrange
        Course course = await SeedCourseAsync(chapters:
        [
            CreateChapter(1, "Chapter 1", 2)
        ]);
        Course? tracked = await Context.Courses.FindAsync(course.Id);

        // Act
        _sut.Delete(tracked!);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        Context.Chapters.Count(c => c.CourseId == course.Id).ShouldBe(0);
        Context.SubChapters.Count().ShouldBe(0);
    }

    [Fact]
    public async Task Update_ExistingCourse_EntityRemainsIntact()
    {
        // Arrange
        Course course = await SeedCourseAsync("Unchanged Topic");
        Course? tracked = await Context.Courses.FindAsync(course.Id);

        // Act
        _sut.Update(tracked!);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        Course? refreshed = await Context.Courses.FindAsync(course.Id);
        refreshed.ShouldNotBeNull();
        refreshed.Topic.ShouldBe("Unchanged Topic");
    }

    private async Task<Course> SeedCourseAsync(
        string topic = "Default Topic",
        IEnumerable<Chapter>? chapters = null)
    {
        Course course = CreateCourse(topic, chapters);
        Context.Courses.Add(course);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return course;
    }

    private static Course CreateCourse(string topic = "Default Topic", IEnumerable<Chapter>? chapters = null)
    {
        List<Chapter> chapterList = chapters?.ToList()
                                    ?? [CreateChapter(1, "Intro")];
        return Course.Create(topic, chapterList);
    }

    private static Chapter CreateChapter(int day, string title, int subChapterCount = 1)
    {
        var subChapters = Enumerable.Range(1, subChapterCount)
            .Select(i => SubChapter.Create($"Sub {i}", $"Summary {i}", $"<p>Theory {i}</p>"))
            .ToList();
        return Chapter.Create(day, title, $"Core concept for {title}", subChapters);
    }
}
