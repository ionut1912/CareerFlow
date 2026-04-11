using CareerFlow.Core.Application.CQRS.Accounts.Commands;
using CareerFlow.Core.Application.CQRS.Accounts.Handlers;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Accounts;

public class AcceptLegalDocCommandHandlerTests : BaseHandlerTest<AcceptLegalDocCommandHandler>
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly AcceptLegalDocCommandHandler _handler;

    public AcceptLegalDocCommandHandlerTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _handler = new AcceptLegalDocCommandHandler(
            _accountRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Theory]
    [InlineData("Terms")]
    [InlineData("Privacy")]
    public async Task Handle_WhenAccountExistsAndValidType_Should_Modify(string type)
    {
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), type);
        var account = TestDataFactory.CreateAccount();
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.AccountId, Ct))
            .ReturnsAsync(account);

        await _handler.Handle(command, Ct);

        _accountRepositoryMock
            .Verify(x => x.Update(account), Times.Once);
        _unitOfWorkMock
            .Verify(x => x.SaveChangesAsync(Ct), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_ShouldNotModify()
    {
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), "Terms");
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.AccountId, Ct))
            .ReturnsAsync((Account?)null);

        var exception = await Should.ThrowAsync<AccountNotFoundException>(() => _handler.Handle(command, Ct));

        exception.Message.ShouldBe($"Contul cu id-ul {command.AccountId} nu a fost gasit");
        _accountRepositoryMock
            .Verify(x => x.Update(It.IsAny<Account>()), Times.Never);
        _loggerMock.VerifyLogError(command.AccountId.ToString(), Times.Once());
        _unitOfWorkMock
            .Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenInvalidType_TrowsLegalDocInvalidTypeException()
    {
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), "invalid");
        var account = TestDataFactory.CreateAccount();
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.AccountId, Ct))
            .ReturnsAsync(account);

        var exception = await Should.ThrowAsync<LegalDocInvalidTypeException>(() => _handler.Handle(command, Ct));

        exception.Message.ShouldBe("Invalid legal document type");
        _accountRepositoryMock
            .Verify(x => x.Update(It.IsAny<Account>()), Times.Never);
        _unitOfWorkMock
            .Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, true)]
    public async Task Handle_WhenDependenciesAreNull_ThrowsArgumentNullException(bool isRepoNull, bool isUnitOfWorkNull,
        bool isLoggerNull)
    {
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), "Terms");

        await Should.ThrowAsync<ArgumentNullException>(() => new AcceptLegalDocCommandHandler(
            isRepoNull ? null! : _accountRepositoryMock.Object,
            isUnitOfWorkNull ? null! : _unitOfWorkMock.Object,
            isLoggerNull ? null! : _loggerMock.Object).Handle(command, Ct));
    }
}