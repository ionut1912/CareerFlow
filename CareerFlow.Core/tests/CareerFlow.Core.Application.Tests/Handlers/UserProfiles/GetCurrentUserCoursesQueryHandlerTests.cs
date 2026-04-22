using CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;
using CareerFlow.Core.Application.CQRS.UserProfiles.Queries;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.UserProfiles;

public class GetCurrentUserCoursesQueryHandlerTests : BaseHandlerTest<GetCurrentUserCoursesQueryHandler>
{
    private readonly Mock<IUserProfileRepository> _repoMock = new();
    private readonly GetCurrentUserCoursesQueryHandler _sut;

    public GetCurrentUserCoursesQueryHandlerTests()
    {
        _sut = new GetCurrentUserCoursesQueryHandler(_repoMock.Object, LoggerMock.Object);
    }

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new GetCurrentUserCoursesQueryHandler(null!, LoggerMock.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() =>
            new GetCurrentUserCoursesQueryHandler(_repoMock.Object, null!));
    }

    [Fact]
    public async Task Handle_ProfileExists_ReturnsDtoWithCorrectAccountId()
    {
        var accountId = Guid.NewGuid();
        var profile = UserProfile.Create(accountId, LearningType.Visual, [UserType.Student]);
        var query = new GetCurrentUserCoursesQuery(accountId);

        _repoMock.Setup(r => r.GetUserCourses(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        UserProfileDto result = await _sut.Handle(query, Ct);

        result.ShouldNotBeNull();
        result.AccountId.ShouldBe(accountId);
    }

    [Fact]
    public async Task Handle_ProfileNotFound_ThrowsAccountNotFoundException()
    {
        var accountId = Guid.NewGuid();
        var query = new GetCurrentUserCoursesQuery(accountId);

        _repoMock.Setup(r => r.GetUserCourses(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        await Should.ThrowAsync<AccountNotFoundException>(() =>
            _sut.Handle(query, Ct));
    }

    [Fact]
    public async Task Handle_ProfileNotFound_LogsErrorWithAccountId()
    {
        var accountId = Guid.NewGuid();
        var query = new GetCurrentUserCoursesQuery(accountId);

        _repoMock.Setup(r => r.GetUserCourses(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProfile?)null);

        await Should.ThrowAsync<AccountNotFoundException>(() =>
            _sut.Handle(query, Ct));

        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(accountId.ToString())),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectAccountId()
    {
        var accountId = Guid.NewGuid();
        var profile = UserProfile.Create(accountId, LearningType.Visual, [UserType.Student]);
        var query = new GetCurrentUserCoursesQuery(accountId);

        _repoMock.Setup(r => r.GetUserCourses(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        await _sut.Handle(query, Ct);

        _repoMock.Verify(r => r.GetUserCourses(accountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_PassesCancellationTokenToRepository()
    {
        var accountId = Guid.NewGuid();
        var profile = UserProfile.Create(accountId, LearningType.Visual, [UserType.Student]);
        var query = new GetCurrentUserCoursesQuery(accountId);


        _repoMock.Setup(r => r.GetUserCourses(accountId, Ct)).ReturnsAsync(profile);

        await _sut.Handle(query, Ct);

        _repoMock.Verify(r => r.GetUserCourses(accountId, Ct), Times.Once);

    }
}
