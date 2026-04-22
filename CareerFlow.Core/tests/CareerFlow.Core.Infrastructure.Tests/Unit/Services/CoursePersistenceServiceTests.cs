using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.Assembly;
using CareerFlow.Core.Domain.ValueObjects;
using CareerFlow.Core.Infrastructure.Services;
using Moq;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Services;

public class CoursePersistenceServiceTests
{
    private readonly Mock<ICourseRepository> _courseRepoMock = new();
    private readonly Mock<IQuizRepository> _quizRepoMock = new();
    private readonly CoursePersistenceService _sut;
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IUserProfileRepository> _userProfileRepoMock = new();

    public CoursePersistenceServiceTests()
    {
        _sut = new CoursePersistenceService(
            _courseRepoMock.Object,
            _userProfileRepoMock.Object,
            _quizRepoMock.Object,
            _uowMock.Object);
    }

    [Fact]
    public void Constructor_NullCourseRepository_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new CoursePersistenceService(
            null!, _userProfileRepoMock.Object, _quizRepoMock.Object, _uowMock.Object));
    }

    [Fact]
    public void Constructor_NullUserProfileRepository_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new CoursePersistenceService(
            _courseRepoMock.Object, null!, _quizRepoMock.Object, _uowMock.Object));
    }

    [Fact]
    public void Constructor_NullQuizRepository_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new CoursePersistenceService(
            _courseRepoMock.Object, _userProfileRepoMock.Object, null!, _uowMock.Object));
    }

    [Fact]
    public void Constructor_NullUnitOfWork_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new CoursePersistenceService(
            _courseRepoMock.Object, _userProfileRepoMock.Object, _quizRepoMock.Object, null!));
    }

    [Fact]
    public async Task PersistAsync_ProfileNotFound_ThrowsUserProfileNotFoundException()
    {
        var userId = Guid.NewGuid();
        _userProfileRepoMock
            .Setup(r => r.GetCurrentUserProfile(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        await Should.ThrowAsync<UserProfileNotFoundException>(() =>
            _sut.PersistAsync(userId, "Topic", CreateValidAssemblyData()));
    }

    [Fact]
    public async Task PersistAsync_ValidData_ReturnsNonEmptyGuid()
    {
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, LearningType.Visual, [UserType.Student]);
        SetupSuccessfulPersist(userId, profile);

        Guid result = await _sut.PersistAsync(userId, "Topic", CreateValidAssemblyData());

        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task PersistAsync_ValidData_BeginsTransaction()
    {
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, LearningType.Visual, [UserType.Student]);
        SetupSuccessfulPersist(userId, profile);

        await _sut.PersistAsync(userId, "Topic", CreateValidAssemblyData());

        _uowMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PersistAsync_ValidData_CommitsTransaction()
    {
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, LearningType.Visual, [UserType.Student]);
        SetupSuccessfulPersist(userId, profile);

        await _sut.PersistAsync(userId, "Topic", CreateValidAssemblyData());

        _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PersistAsync_ValidData_SavesChangesAtLeastOnce()
    {
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, LearningType.Visual, [UserType.Student]);
        SetupSuccessfulPersist(userId, profile);

        await _sut.PersistAsync(userId, "Topic", CreateValidAssemblyData());

        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task PersistAsync_ValidData_AddsCourseToRepository()
    {
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, LearningType.Visual, [UserType.Student]);
        SetupSuccessfulPersist(userId, profile);

        await _sut.PersistAsync(userId, "Topic", CreateValidAssemblyData());

        _courseRepoMock.Verify(r => r.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PersistAsync_ValidData_UpdatesUserProfile()
    {
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, LearningType.Visual, [UserType.Student]);
        SetupSuccessfulPersist(userId, profile);

        await _sut.PersistAsync(userId, "Topic", CreateValidAssemblyData());

        _userProfileRepoMock.Verify(r => r.Update(It.IsAny<UserProfile>()), Times.Once);
    }

    [Fact]
    public async Task PersistAsync_CourseAddThrows_RollsBackTransaction()
    {
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, LearningType.Visual, [UserType.Student]);

        _userProfileRepoMock.Setup(r => r.GetCurrentUserProfile(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _uowMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _courseRepoMock.Setup(r => r.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
        _uowMock.Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.PersistAsync(userId, "Topic", CreateValidAssemblyData()));

        _uowMock.Verify(u => u.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PersistAsync_CourseAddThrows_NeverCommitsTransaction()
    {
        var userId = Guid.NewGuid();
        var profile = UserProfile.Create(userId, LearningType.Visual, [UserType.Student]);

        _userProfileRepoMock.Setup(r => r.GetCurrentUserProfile(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _uowMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _courseRepoMock.Setup(r => r.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB error"));
        _uowMock.Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            _sut.PersistAsync(userId, "Topic", CreateValidAssemblyData()));

        _uowMock.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupSuccessfulPersist(Guid userId, UserProfile profile)
    {
        _userProfileRepoMock.Setup(r => r.GetCurrentUserProfile(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _uowMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _courseRepoMock.Setup(r => r.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userProfileRepoMock.Setup(r => r.Update(It.IsAny<UserProfile>()));
        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _quizRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<List<QuizQuestion>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    private static List<ChapterAssemblyModel> CreateValidAssemblyData()
    {
        return
        [
            new ChapterAssemblyModel(1, "Chapter 1", "Core Concept",
                [new SubchapterAssemblyModel("Sub 1", "Summary", "<p>Theory</p>", [])], [])
        ];
    }
}
