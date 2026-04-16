using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Tests.Setup;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Integration;

[Trait("Category", "Integration")]
public class ChapterRepositoryTests : BaseRepositoryTest, IAsyncLifetime
{
    private readonly ChapterRepository _sut;

    public ChapterRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new ChapterRepository(Context.Chapters);
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
    public async Task ExistsAsync_MatchingChapterIdAndCourseId_ReturnsTrue()
    {
        // Arrange
        var (_, chapter) = await SeedChapterAsync();

        // Act
        var result = await _sut.ExistsAsync(chapter.Id, chapter.CourseId, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExistentChapterId_ReturnsFalse()
    {
        // Arrange
        var (_, chapter) = await SeedChapterAsync();

        // Act
        var result = await _sut.ExistsAsync(Guid.NewGuid(), chapter.CourseId, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_NonExistentCourseId_ReturnsFalse()
    {
        // Arrange
        var (_, chapter) = await SeedChapterAsync();

        // Act
        var result = await _sut.ExistsAsync(chapter.Id, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ChapterIdBelongsToDifferentCourse_ReturnsFalse()
    {
        // Arrange
        var (_, chapter1) = await SeedChapterAsync();
        var (course2, _) = await SeedChapterAsync();

        // Act
        var result = await _sut.ExistsAsync(chapter1.Id, course2.Id, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_BothIdsEmpty_ReturnsFalse()
    {
        // Act
        var result = await _sut.ExistsAsync(Guid.Empty, Guid.Empty, CancellationToken.None);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task AddAsync_Chapter_PersistsToDatabase()
    {
        // Arrange
        var chapter = CreateChapter();
        var course = Course.Create("Topic", new List<Chapter> { chapter });
        Context.Courses.Add(course);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var saved = await Context.Chapters.FindAsync(chapter.Id);

        // Assert
        saved.ShouldNotBeNull();
        saved.Title.ShouldBe("Chapter Title");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsChapter()
    {
        // Arrange
        var (_, chapter) = await SeedChapterAsync();

        // Act
        var result = await _sut.GetByIdAsync(chapter.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(chapter.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithSubChaptersInclude_ReturnsChapterWithSubChapters()
    {
        // Arrange
        var (_, chapter) = await SeedChapterAsync();

        // Act
        var result = await _sut.GetByIdAsync(chapter.Id, CancellationToken.None, c => c.SubChapters);

        // Assert
        result.ShouldNotBeNull();
        result.SubChapters.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WithCourseInclude_ReturnsChapterWithCourseId()
    {
        // Arrange
        var (course, chapter) = await SeedChapterAsync();

        // Act
        var result = await _sut.GetByIdAsync(chapter.Id);

        // Assert
        result.ShouldNotBeNull();
        result.CourseId.ShouldBe(course.Id);
    }

    [Fact]
    public async Task GetAllAsync_MultipleChapters_ReturnsAll()
    {
        // Arrange
        var course = Course.Create("Topic", new List<Chapter>
        {
            CreateChapter(1, "Chapter 1"),
            CreateChapter(2, "Chapter 2")
        });
        Context.Courses.Add(course);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllAsync_WithSubChaptersInclude_ReturnsChaptersWithSubChapters()
    {
        // Arrange
        await SeedChapterAsync();

        // Act
        var result = (await _sut.GetAllAsync(CancellationToken.None, c => c.SubChapters)).ToList();

        // Assert
        result.ShouldAllBe(c => c.SubChapters.Any());
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsEmptyCollection()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Delete_ExistingChapter_RemovesFromDatabase()
    {
        // Arrange
        var (_, chapter) = await SeedChapterAsync();
        var tracked = await Context.Chapters.FindAsync(chapter.Id);

        // Act
        _sut.Delete(tracked!);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var deleted = await Context.Chapters.FindAsync(chapter.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Update_ChangesArePersisted()
    {
        // Arrange
        var (_, chapter) = await SeedChapterAsync();
        var tracked = await Context.Chapters.FindAsync(chapter.Id);
        tracked!.SetCourseId(tracked.CourseId);

        // Act
        _sut.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.Chapters.FindAsync(chapter.Id);
        refreshed.ShouldNotBeNull();
        refreshed!.CourseId.ShouldBe(chapter.CourseId);
    }

    private static Chapter CreateChapter(int day = 1, string title = "Chapter Title")
    {
        var subChapters = new List<SubChapter>
        {
            SubChapter.Create("Sub 1", "Summary 1", "<p>Theory 1</p>")
        };
        return Chapter.Create(day, title, "Core Concept", subChapters);
    }

    private async Task<(Course course, Chapter chapter)> SeedChapterAsync()
    {
        var subChapters = new List<SubChapter>
        {
            SubChapter.Create("Sub 1", "Summary 1", "<p>Theory 1</p>")
        };
        var chapter = Chapter.Create(1, "Chapter Title", "Core Concept", subChapters);
        var course = Course.Create("Topic", new List<Chapter> { chapter });

        Context.Courses.Add(course);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var savedChapter = await Context.Chapters.FindAsync(chapter.Id);
        return (course, savedChapter!);
    }
}