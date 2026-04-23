using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;

using Moq;

using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.UserProfiles;

public class DeleteUserProfileCommandHandlerTests : BaseHandlerTest<DeleteUserProfileCommandHandler>
{
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly DeleteUserProfileCommandHandler _handler;
    private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;

    public DeleteUserProfileCommandHandlerTests()
    {
        _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
        _cacheServiceMock = new Mock<ICacheService>();

        _cacheServiceMock
            .Setup(x => x.RemoveByPatternAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _handler = new DeleteUserProfileCommandHandler(
            _userProfileRepositoryMock.Object,
            LoggerMock.Object,
            UnitOfWorkMock.Object,
            _cacheServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ProfileExists_ShouldDeleteProfile()
    {
        // Arrange
        var userProfile =
            UserProfile.Create(Guid.NewGuid(), LearningType.Auditory, [UserType.HobbyLearner], "testDomain");
        var request = new DeleteUserProfileCommand(Guid.NewGuid());

        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, Ct))
            .ReturnsAsync(userProfile);

        // Act
        await _handler.Handle(request, Ct);

        // Assert
        _userProfileRepositoryMock.Verify(x => x.Delete(userProfile), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(Ct), Times.Once);
    }

    [Fact]
    public async Task Handle_ProfileExists_ShouldInvalidateAllCourseCachePatterns()
    {
        // Arrange
        var userProfile =
            UserProfile.Create(Guid.NewGuid(), LearningType.Auditory, [UserType.HobbyLearner], "testDomain");
        var request = new DeleteUserProfileCommand(Guid.NewGuid());

        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, Ct))
            .ReturnsAsync(userProfile);

        // Act
        await _handler.Handle(request, Ct);

        // Assert
        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync("course:analyze"), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync("course:chapters"), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync("course:skeleton"), Times.Once);
        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync("course:expand:"), Times.Once);
    }

    [Fact]
    public async Task Handle_ProfileDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var request = new DeleteUserProfileCommand(Guid.NewGuid());

        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, Ct))
            .ReturnsAsync((UserProfile?)null);

        // Act
        UserProfileNotFoundException exception =
            await Should.ThrowAsync<UserProfileNotFoundException>(() => _handler.Handle(request, Ct));

        // Assert
        exception.Message.ShouldBe($"Profilul cu id-ul {request.Id} nu a fost gasit");
        _userProfileRepositoryMock.Verify(x => x.Delete(It.IsAny<UserProfile>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(Ct), Times.Never);
        LoggerMock.VerifyLogError(request.Id.ToString(), Times.Once());
    }

    [Fact]
    public async Task Handle_ProfileDoesNotExist_ShouldNotInvalidateCache()
    {
        // Arrange
        var request = new DeleteUserProfileCommand(Guid.NewGuid());

        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, Ct))
            .ReturnsAsync((UserProfile?)null);

        // Act
        await Should.ThrowAsync<UserProfileNotFoundException>(() => _handler.Handle(request, Ct));

        // Assert
        _cacheServiceMock.Verify(x => x.RemoveByPatternAsync(It.IsAny<string>()), Times.Never);
    }
}
