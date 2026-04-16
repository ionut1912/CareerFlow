using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Tests.Integration.Setup;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class QuizRepositoryTests : BaseRepositoryTest, IAsyncLifetime
{
    private readonly QuizRepository _sut;

    public QuizRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new QuizRepository(Context.QuizQuestions);
    }

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Context.QuizQuestions.RemoveRange(Context.QuizQuestions);
        Context.SubChapters.RemoveRange(Context.SubChapters);
        Context.Chapters.RemoveRange(Context.Chapters);
        Context.Courses.RemoveRange(Context.Courses);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task AddAsync_Question_PersistsToDatabase()
    {
        // Arrange
        var question = CreateQuestion();

        // Act
        await _sut.AddAsync(question, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.QuizQuestions.FindAsync(question.Id);
        saved.ShouldNotBeNull();
        saved!.Question.ShouldBe("What is DI?");
        saved.CorrectAnswer.ShouldBe("Dependency Injection");
        saved.Options.ShouldBe(new List<string> { "Dependency Injection", "Direct Invocation" });
    }

    [Fact]
    public async Task AddRangeAsync_SingleQuestion_PersistsToDatabase()
    {
        // Arrange
        var questions = new List<QuizQuestion> { CreateQuestion() };

        // Act
        await _sut.AddRangeAsync(questions, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        Context.QuizQuestions.ToList().Count.ShouldBe(1);
    }

    [Fact]
    public async Task AddRangeAsync_MultipleQuestions_PersistsAllToDatabase()
    {
        // Arrange
        var questions = new List<QuizQuestion>
        {
            CreateQuestion(),
            CreateQuestion("What is SRP?"),
            CreateQuestion("What is TDD?")
        };

        // Act
        await _sut.AddRangeAsync(questions, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = Context.QuizQuestions.ToList();
        saved.Count.ShouldBe(3);
        saved.ShouldContain(q => q.Question == "What is DI?");
        saved.ShouldContain(q => q.Question == "What is SRP?");
        saved.ShouldContain(q => q.Question == "What is TDD?");
    }

    [Fact]
    public async Task AddRangeAsync_EmptyList_PersistsNothing()
    {
        // Arrange
        var questions = new List<QuizQuestion>();

        // Act
        await _sut.AddRangeAsync(questions, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        Context.QuizQuestions.ToList().ShouldBeEmpty();
    }

    [Fact]
    public async Task AddRangeAsync_QuestionWithChapterId_PersistsChapterId()
    {
        // Arrange
        var (_, chapter) = await SeedChapterAsync();
        var questions = new List<QuizQuestion>
        {
            CreateQuestion(chapterId: chapter.Id)
        };

        // Act
        await _sut.AddRangeAsync(questions, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = Context.QuizQuestions.First();
        saved.ChapterId.ShouldBe(chapter.Id);
    }

    [Fact]
    public async Task AddRangeAsync_QuestionWithSubChapterId_PersistsSubChapterId()
    {
        // Arrange
        var (_, _, subChapter) = await SeedSubChapterAsync();
        var questions = new List<QuizQuestion>
        {
            CreateQuestion(subChapterId: subChapter.Id)
        };

        // Act
        await _sut.AddRangeAsync(questions, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = Context.QuizQuestions.First();
        saved.SubChapterId.ShouldBe(subChapter.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsQuestion()
    {
        // Arrange
        var question = await SeedQuestionAsync();

        // Act
        var result = await _sut.GetByIdAsync(question.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(question.Id);
        result.Question.ShouldBe(question.Question);
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
    public async Task GetByIdAsync_QuestionLinkedToChapter_ChapterIdIsSet()
    {
        // Arrange
        var (_, chapter) = await SeedChapterAsync();
        var question = await SeedQuestionAsync(chapterId: chapter.Id);

        // Act
        var result = await _sut.GetByIdAsync(question.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.ChapterId.ShouldBe(chapter.Id);
    }

    [Fact]
    public async Task GetByIdAsync_QuestionLinkedToSubChapter_SubChapterIdIsSet()
    {
        // Arrange
        var (_, _, subChapter) = await SeedSubChapterAsync();
        var question = await SeedQuestionAsync(subChapterId: subChapter.Id);

        // Act
        var result = await _sut.GetByIdAsync(question.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.SubChapterId.ShouldBe(subChapter.Id);
    }

    [Fact]
    public async Task GetAllAsync_MultipleQuestions_ReturnsAll()
    {
        // Arrange
        await SeedQuestionAsync();
        await SeedQuestionAsync("What is SRP?");

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(q => q.Question == "What is DI?");
        result.ShouldContain(q => q.Question == "What is SRP?");
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
    public async Task Delete_ExistingQuestion_RemovesFromDatabase()
    {
        // Arrange
        var question = await SeedQuestionAsync();
        var tracked = await Context.QuizQuestions.FindAsync(question.Id);

        // Act
        _sut.Delete(tracked!);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var deleted = await Context.QuizQuestions.FindAsync(question.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Update_ExistingQuestion_EntityRemainsIntact()
    {
        // Arrange
        var question = await SeedQuestionAsync();
        var tracked = await Context.QuizQuestions.FindAsync(question.Id);

        // Act
        _sut.Update(tracked!);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.QuizQuestions.FindAsync(question.Id);
        refreshed.ShouldNotBeNull();
        refreshed!.Question.ShouldBe(question.Question);
        refreshed.CorrectAnswer.ShouldBe(question.CorrectAnswer);
    }

    private async Task<QuizQuestion> SeedQuestionAsync(
        string questionText = "What is DI?",
        Guid? chapterId = null,
        Guid? subChapterId = null)
    {
        var question = CreateQuestion(questionText, chapterId, subChapterId);
        Context.QuizQuestions.Add(question);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return question;
    }

    private async Task<(Course course, Chapter chapter)> SeedChapterAsync()
    {
        var subChapter = SubChapter.Create("Sub 1", "Summary 1", "<p>Theory 1</p>");
        var chapter = Chapter.Create(1, "Chapter 1", "Core Concept", new List<SubChapter> { subChapter });
        var course = Course.Create("Topic", new List<Chapter> { chapter });
        Context.Courses.Add(course);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        var savedChapter = await Context.Chapters.FindAsync(chapter.Id);
        return (course, savedChapter!);
    }

    private async Task<(Course course, Chapter chapter, SubChapter subChapter)> SeedSubChapterAsync()
    {
        var subChapter = SubChapter.Create("Sub 1", "Summary 1", "<p>Theory 1</p>");
        var chapter = Chapter.Create(1, "Chapter 1", "Core Concept", new List<SubChapter> { subChapter });
        var course = Course.Create("Topic", new List<Chapter> { chapter });
        Context.Courses.Add(course);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        var savedSubChapter = await Context.SubChapters.FindAsync(subChapter.Id);
        return (course, chapter, savedSubChapter!);
    }

    private static QuizQuestion CreateQuestion(
        string questionText = "What is DI?",
        Guid? chapterId = null,
        Guid? subChapterId = null)
    {
        return QuizQuestion.Create(
            questionText,
            new List<string> { "Dependency Injection", "Direct Invocation" },
            "Dependency Injection",
            chapterId,
            subChapterId);
    }
}