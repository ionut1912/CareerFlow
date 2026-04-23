using CareerFlow.Core.Application.CQRS.Accounts.Commands;
using CareerFlow.Core.Application.CQRS.Accounts.Handlers;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;

using Microsoft.Extensions.Logging;

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
            UnitOfWorkMock.Object,
            LoggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserExists_DeletesUser()
    {
        // Arrange
        Account account = TestDataFactory.CreateAccount();
        var command = new DeleteAccountCommand(account.Id);
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(account.Id, Ct)).ReturnsAsync(account);

        // Act
        await _handler.Handle(command, Ct);

        // Assert
        _accountRepositoryMock.Verify(x => x.Delete(account), Times.Once);
        UnitOfWorkMock.VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsAccountNotFoundException()
    {
        // Arrange
        var command = new DeleteAccountCommand(Guid.NewGuid());
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.Id, Ct)).ReturnsAsync((Account?)null);

        // Act
        AccountNotFoundException exception =
            await Should.ThrowAsync<AccountNotFoundException>(() => _handler.Handle(command, Ct));

        // Assert
        exception.Message.ShouldContain(command.Id.ToString());
        LoggerMock.VerifyLogError(command.Id.ToString(), Times.Once());
        _accountRepositoryMock.Verify(x => x.Delete(It.IsAny<Account>()), Times.Never);
        UnitOfWorkMock.VerifySaveChanges(Times.Never());
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, true)]
    public async Task Handle_WhenDependenciesAreNull_ThrowsArgumentNullException(
        bool isAccountRepoNull, bool isUowNull, bool isLoggerNull)
    {
        // Arrange
        var command = new DeleteAccountCommand(Guid.NewGuid());

        // Use ! to suppress the possible null reference argument warning
        IAccountRepository repo = isAccountRepoNull ? null! : _accountRepositoryMock.Object;
        IUnitOfWork uow = isUowNull ? null! : UnitOfWorkMock.Object;
        ILogger<DeleteAccountCommandHandler> logger = isLoggerNull ? null! : LoggerMock.Object;

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            var handler = new DeleteAccountCommandHandler(repo, uow, logger);
            await handler.Handle(command, Ct);
        });
    }
}
