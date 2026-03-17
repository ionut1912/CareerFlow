using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.UserProfiles;

public class CreateUserProfileCommandHandlerTests : BaseHandlerTest<CreateUserProfileCommandHandler>
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly CreateUserProfileCommandHandler _handler;
    private readonly Mock<IUserProfileRepository> _userProfileRepositroyMock;

    public CreateUserProfileCommandHandlerTests()
    {
        _userProfileRepositroyMock = new Mock<IUserProfileRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _handler = new CreateUserProfileCommandHandler(
            _userProfileRepositroyMock.Object,
            _accountRepositoryMock.Object,
            _loggerMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task HandleAsync_CreateUserProfile()
    {
        //Arrange
        var accountToReturn = Account.Create("testEmail", "testPassword", "testUsername", "testName");
        var request = new CreateUserProfileCommand(accountToReturn.Id, "Visual", ["HobbyLearner"], "testDomain");
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(request.AccountId, Ct))
            .ReturnsAsync(accountToReturn);

        //Act
        await _handler.Handle(request, Ct);

        //Assert
        _accountRepositoryMock
            .Verify(x => x.GetByIdAsync(request.AccountId, Ct), Times.Once);

        _userProfileRepositroyMock.Verify(x => x.AddAsync(It.IsAny<UserProfile>()), Times.Once);
        _unitOfWorkMock
            .Verify(x => x.SaveChangesAsync(Ct), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AccountNotFound_ThrowsException()
    {
        var request = new CreateUserProfileCommand(Guid.NewGuid(), "Visual", ["HobbyLearner"], "testDomain");
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(request.AccountId, Ct))
            .ReturnsAsync((Account?)null);

        //Act
        var exception = await Should.ThrowAsync<AccountNotFoundException>(() => _handler.Handle(request, Ct));

        //Assert
        exception.Message.ShouldBe($"Contul cu id-ul {request.AccountId} nu a fost gasit");
        _accountRepositoryMock
            .Verify(x => x.GetByIdAsync(request.AccountId, Ct), Times.Once);

        _userProfileRepositroyMock.Verify(x => x.AddAsync(It.IsAny<UserProfile>()), Times.Never);
        _unitOfWorkMock
            .Verify(x => x.SaveChangesAsync(Ct), Times.Never);
        _loggerMock.VerifyLogError(request.AccountId.ToString(), Times.Once());
    }

    [Fact]
    public async Task HandleAsync_InvalidLearningType_ThrowsException()
    {
        //Arrange
        var accountToReturn = Account.Create("testEmail", "testPassword", "testUsername", "testName");
        var request = new CreateUserProfileCommand(accountToReturn.Id, "test", ["HobbyLearner"], "testDomain");
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(request.AccountId, Ct))
            .ReturnsAsync(accountToReturn);

        //Act
        var exception = await Should.ThrowAsync<InvalidLearningTypeException>(() => _handler.Handle(request, Ct));

        //Assert
        exception.Message.ShouldBe($"Tipul de invatare {request.LearningType} e invalid");
        _accountRepositoryMock
            .Verify(x => x.GetByIdAsync(request.AccountId, Ct), Times.Once);

        _userProfileRepositroyMock.Verify(x => x.AddAsync(It.IsAny<UserProfile>()), Times.Never);
        _unitOfWorkMock
            .Verify(x => x.SaveChangesAsync(Ct), Times.Never);
    }


    [Fact]
    public async Task HandleAsync_InvalidUserType_ThrowsException()
    {
        //Arrange
        var accountToReturn = Account.Create("testEmail", "testPassword", "testUsername", "testName");
        var request = new CreateUserProfileCommand(accountToReturn.Id, "Visual", ["test"], "testDomain");
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(request.AccountId, Ct))
            .ReturnsAsync(accountToReturn);

        //Act
        var exception = await Should.ThrowAsync<InvalidUserTypeException>(() => _handler.Handle(request, Ct));

        //Assert
        exception.Message.ShouldBe($"Tipul {request.UserTypes[0]} este invalid");
        _accountRepositoryMock
            .Verify(x => x.GetByIdAsync(request.AccountId, Ct), Times.Once);

        _userProfileRepositroyMock.Verify(x => x.AddAsync(It.IsAny<UserProfile>()), Times.Never);
        _unitOfWorkMock
            .Verify(x => x.SaveChangesAsync(Ct), Times.Never);
    }
}