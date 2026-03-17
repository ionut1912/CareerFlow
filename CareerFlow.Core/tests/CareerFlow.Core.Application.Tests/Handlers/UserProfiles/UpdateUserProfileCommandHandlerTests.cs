using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.UserProfiles;

public class UpdateUserProfileCommandHandlerTests : BaseHandlerTest<UpdateUserProfileCommandHandler>
{
    private readonly UpdateUserProfileCommandHandler _handler;
    private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;

    public UpdateUserProfileCommandHandlerTests()
    {
        _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
        _handler = new UpdateUserProfileCommandHandler(
            _userProfileRepositoryMock.Object,
            _loggerMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingProfile_UpdatesUserProfile()
    {
        //Arrange
        var request = new UpdateUserProfileCommand(Guid.NewGuid(), "visual", ["student"], "student");
        var existingUserProfile =
            UserProfile.Create(Guid.NewGuid(), LearningType.Auditory, [UserType.JobSearcher], "Chemistry");
        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, Ct))
            .ReturnsAsync(existingUserProfile);

        //Act
        await _handler.Handle(request, Ct);

        //Assert
        _userProfileRepositoryMock
            .Verify(x => x.Update(It.IsAny<UserProfile>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProfileNotFound_ThrowsException()
    {
        //Arrange
        var request = new UpdateUserProfileCommand(Guid.NewGuid(), "visual", ["student"], "student");
        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, Ct))
            .ReturnsAsync((UserProfile?)null);

        //Act
        var exception = await Should.ThrowAsync<UserProfileNotFoundException>(() => _handler.Handle(request, Ct));

        //Assert
        exception.Message.ShouldBe($"Profilul cu id-ul {request.Id} nu a fost gasit");
        _userProfileRepositoryMock
            .Verify(x => x.Update(It.IsAny<UserProfile>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _loggerMock.VerifyLogError(request.Id.ToString(), Times.Once());
    }

    [Fact]
    public async Task Handle_InvalidLearningType_ThrowsException()
    {
        //Arrange
        var request = new UpdateUserProfileCommand(Guid.NewGuid(), "test", ["student"], "student");
        var existingUserProfile =
            UserProfile.Create(Guid.NewGuid(), LearningType.Auditory, [UserType.JobSearcher], "Chemistry");
        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, Ct))
            .ReturnsAsync(existingUserProfile);

        //Act
        var exception = await Should.ThrowAsync<InvalidLearningTypeException>(() => _handler.Handle(request, Ct));

        //Assert
        exception.Message.ShouldBe($"Tipul de invatare {request.LearningType} e invalid");
        _userProfileRepositoryMock
            .Verify(x => x.Update(It.IsAny<UserProfile>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidUserType_ThrowsException()
    {
        //Arrange
        var request = new UpdateUserProfileCommand(Guid.NewGuid(), "visual", ["test"], "student");
        var existingUserProfile =
            UserProfile.Create(Guid.NewGuid(), LearningType.Auditory, [UserType.JobSearcher], "Chemistry");
        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, Ct))
            .ReturnsAsync(existingUserProfile);

        //Act
        var exception = await Should.ThrowAsync<InvalidUserTypeException>(() => _handler.Handle(request, Ct));

        //Assert
        exception.Message.ShouldBe($"Tipul {request.UserTypes[0]} este invalid");
        _userProfileRepositoryMock
            .Verify(x => x.Update(It.IsAny<UserProfile>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}