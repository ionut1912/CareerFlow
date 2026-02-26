using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Application.CQRS.Accounts.Handler;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Accounts;

public class DeleteAccountCommandHandlerTests : BaseHandlerTest<DeleteAccountCommandHandler>
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly DeleteAccountCommandHandler _handler;

    public DeleteAccountCommandHandlerTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _handler = new DeleteAccountCommandHandler(
            _accountRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_DeletesUser()
    {
        // Arrange
        var account = TestDataFactory.CreateAccount();
        var command = new DeleteAccountCommand(account.Id);
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, Ct)).ReturnsAsync(account);

        // Act
        await _handler.Handle(command, Ct);

        // Assert
        _accountRepositoryMock.Verify(x => x.Delete(account), Times.Once);
        _unitOfWorkMock.VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsAccountNotFoundException()
    {
        // Arrange
        var command = new DeleteAccountCommand(Guid.NewGuid());
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.Id, Ct)).ReturnsAsync((Account?)null);

        // Act
        var exception = await Should.ThrowAsync<AccountNotFoundException>(() => _handler.Handle(command, Ct));

        // Assert
        _loggerMock.VerifyLogError(command.Id.ToString(), Times.Once());
        _accountRepositoryMock.Verify(x => x.Delete(It.IsAny<Account>()), Times.Never);
        _unitOfWorkMock.VerifySaveChanges(Times.Never());
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, true)]
    public async Task Handle_WhenDependenciesAreNull_ThrowsArgumentNullException(
        bool isAccountRepoNull, bool isUowNull, bool isLoggerNull)
    {
        //Act
        var command = new DeleteAccountCommand(Guid.NewGuid());

        //Arrange&Assert
        await Should.ThrowAsync<ArgumentNullException>(() => new DeleteAccountCommandHandler(
            isAccountRepoNull ? null : _accountRepositoryMock.Object,
            isUowNull ? null : _unitOfWorkMock.Object,
            isLoggerNull ? null : _loggerMock.Object).Handle(command, Ct));
    }
}