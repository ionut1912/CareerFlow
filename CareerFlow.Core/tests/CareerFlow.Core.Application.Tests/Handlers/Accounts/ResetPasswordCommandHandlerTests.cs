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
        var command = new ResetPasswordCommand(account.Email, "newPassword");
        _accountRepositoryMock.Setup(x => x.GetAccountByEmailAsync(account.Email, Ct)).ReturnsAsync(account);

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
        var command = new ResetPasswordCommand("testmail", "newPassword");
        _accountRepositoryMock.Setup(x => x.GetAccountByEmailAsync(command.Email, Ct)).ReturnsAsync((Account?)null);

        // Act
        var exception = await Should.ThrowAsync<AccountNotFoundException>(() => _handler.Handle(command, Ct));

        // Assert
        _loggerMock.VerifyLogError(command.Email, Times.Once());
        _unitOfWorkMock.VerifySaveChanges(Times.Never());
    }
}