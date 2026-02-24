using System.Reflection.Emit;
using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Application.CQRS.Accounts.Handler;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Accounts;

public class ResetPasswordCommandHandlerTests : BaseHandlerTest<ResetPasswordCommandHandler>
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly ResetPasswordCommandHandler _handler;
    private readonly Mock<IPasswordService> _passwordServiceMock;

    public ResetPasswordCommandHandlerTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _handler = new ResetPasswordCommandHandler(
            _loggerMock.Object,
            _accountRepositoryMock.Object,
            _passwordServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenAccountExists_ResetsPassword()
    {
        // Arrange
        var account = TestDataFactory.CreateAccount();
        account.SetResetPasswordExipiresAt(DateTime.UtcNow.AddHours(1));
        var command = new ResetPasswordCommand(account.Email, "newPassword","token");
        _accountRepositoryMock.Setup(x => x.GetAccountByEmailAsync(account.Email, Ct)).ReturnsAsync(account);
        _passwordServiceMock.Setup(x=>x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        // Act
        await _handler.Handle(command, Ct);

        // Assert
        _passwordServiceMock.Verify(x => x.HashPassword(command.NewPassword),
            Times.Once); // Assuming handler calls this
        _accountRepositoryMock.Verify(x => x.Update(account), Times.Once);
        _unitOfWorkMock.VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_ThrowsAccountNotFoundException()
    {
        // Arrange
        var command = new ResetPasswordCommand("testmail", "newPassword","token");
        _accountRepositoryMock.Setup(x => x.GetAccountByEmailAsync(command.Email, Ct)).ReturnsAsync((Account?)null);

        // Act
        var exception = await Should.ThrowAsync<AccountNotFoundException>(() => _handler.Handle(command, Ct));

        // Assert
        _loggerMock.VerifyLogError(command.Email, Times.Once());
        _unitOfWorkMock.VerifySaveChanges(Times.Never());
    }
    
    [Fact]
    public async Task Handle_WhenTokenNoMatch_ThrowsPasswordMismatchException()
    {
        //Arrange
        var account = TestDataFactory.CreateAccount();
        var command = new ResetPasswordCommand(account.Email, "newPassword","token");
        _accountRepositoryMock.Setup(x => x.GetAccountByEmailAsync(account.Email, Ct)).ReturnsAsync(account);
        _passwordServiceMock.Setup(x=>x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);
        
        //Act
        var exception = await Should.ThrowAsync<PasswordNotMatchException>(() => _handler.Handle(command, Ct));
        
        //Assert
        exception.Message.ShouldBe("Tokenurile nu sunt la fel");
        _loggerMock.VerifyLogError("Tokenurile nu sunt la fel", Times.Once());
        _unitOfWorkMock.VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenTokenExpired_ThrowsIInvalidFieldException()
    {
        //Arrange
        var account = TestDataFactory.CreateAccount();
        account.SetResetPasswordExipiresAt(DateTime.Now.AddDays(-1));
        var command = new ResetPasswordCommand(account.Email, "newPassword","token");
        _accountRepositoryMock.Setup(x => x.GetAccountByEmailAsync(account.Email, Ct)).ReturnsAsync(account);
        _passwordServiceMock.Setup(x=>x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);
        
        //Act
        var exception=await Should.ThrowAsync<InvalidFieldException>(() => _handler.Handle(command, Ct));
        
        //Assert
        exception.Message.ShouldBe("Tokenul e expirat");
        _loggerMock.VerifyLogError("Tokenul e expirat", Times.Once());
        _unitOfWorkMock.VerifySaveChanges(Times.Never());
    }
}