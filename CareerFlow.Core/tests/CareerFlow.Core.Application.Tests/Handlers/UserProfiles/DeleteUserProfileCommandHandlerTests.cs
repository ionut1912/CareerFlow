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

public class DeleteUserProfileCommandHandlerTests : BaseHandlerTest<DeleteUserProfileCommandHandler>
{
    private readonly DeleteUserProfileCommandHandler _handler;
    private readonly Mock<IUserProfileRepository> _userProfileRepositoryMock;

    public DeleteUserProfileCommandHandlerTests()
    {
        _userProfileRepositoryMock = new Mock<IUserProfileRepository>();
        _handler = new DeleteUserProfileCommandHandler(
            _userProfileRepositoryMock.Object,
            _loggerMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ProfileExists_DeleteProfile()
    {
        //Arrange
        var userProfileToReturn =
            UserProfile.Create(Guid.NewGuid(), LearningType.Auditory, [UserType.HobbyLearner], "testDomain");
        var request = new DeleteUserProfileCommand(Guid.NewGuid());
        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, Ct))
            .ReturnsAsync(userProfileToReturn);

        //Act
        await _handler.Handle(request, Ct);

        //Assert
        _userProfileRepositoryMock
            .Verify(x => x.Delete(userProfileToReturn), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(Ct), Times.Once);
    }

    [Fact]
    public async Task Handle_ProfileDoesNotExist_ThrowsException()
    {
        //Arrange
        var request = new DeleteUserProfileCommand(Guid.NewGuid());
        _userProfileRepositoryMock
            .Setup(x => x.GetByIdAsync(request.Id, Ct))
            .ReturnsAsync((UserProfile?)null);

        //Act
        var exception = await Should.ThrowAsync<UserProfileNotFoundException>(() => _handler.Handle(request, Ct));

        //Assert
        exception.Message.ShouldBe($"Profilul cu id-ul {request.Id} nu a fost gasit");
        _userProfileRepositoryMock
            .Verify(x => x.Delete(It.IsAny<UserProfile>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(Ct), Times.Never);
        _loggerMock.VerifyLogError(request.Id.ToString(), Times.Once());
    }
}