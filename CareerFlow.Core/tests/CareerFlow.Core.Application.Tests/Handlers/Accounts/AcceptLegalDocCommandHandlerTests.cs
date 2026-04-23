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
            UnitOfWorkMock.Object,
            LoggerMock.Object);
    }

    [Theory]
    [InlineData("Terms")]
    [InlineData("Privacy")]
    public async Task Handle_WhenAccountExistsAndValidType_CallsUpdateAndSave(string type)
    {
        //Arrange
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), type);
        Account account = TestDataFactory.CreateAccount();
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.AccountId, Ct))
            .ReturnsAsync(account);

        //Act
        await _handler.Handle(command, Ct);

        //Assert
        _accountRepositoryMock.Verify(x => x.Update(account), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(Ct), Times.Once);
    }

    [Theory]
    [InlineData("terms")]
    [InlineData("TERMS")]
    [InlineData("TeRmS")]
    [InlineData("privacy")]
    [InlineData("PRIVACY")]
    [InlineData("PrIvAcY")]
    public async Task Handle_WhenTypeIsDifferentCase_CallsUpdateAndSave(string type)
    {
        //Arrange
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), type);
        Account account = TestDataFactory.CreateAccount();
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.AccountId, Ct))
            .ReturnsAsync(account);

        //Act
        await _handler.Handle(command, Ct);

        //Assert
        _accountRepositoryMock.Verify(x => x.Update(account), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(Ct), Times.Once);
    }

    [Theory]
    [InlineData("  Terms  ")]
    [InlineData("  Privacy  ")]
    [InlineData(" terms ")]
    [InlineData(" privacy ")]
    public async Task Handle_WhenTypeHasWhitespace_CallsUpdateAndSave(string type)
    {
        //Arrange
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), type);
        Account account = TestDataFactory.CreateAccount();
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.AccountId, Ct))
            .ReturnsAsync(account);

        //Act
        await _handler.Handle(command, Ct);

        //Assert
        _accountRepositoryMock.Verify(x => x.Update(account), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(Ct), Times.Once);
    }

    [Theory]
    [InlineData("Terms")]
    [InlineData("terms")]
    [InlineData("TERMS")]
    public async Task Handle_WhenTypeIsTerms_CallsAcceptTermsOnAccount(string type)
    {
        //Arrange
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), type);
        Account account = TestDataFactory.CreateAccount();
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.AccountId, Ct))
            .ReturnsAsync(account);

        //Act
        await _handler.Handle(command, Ct);

        //Assert
        account.TermsAccepted.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Privacy")]
    [InlineData("privacy")]
    [InlineData("PRIVACY")]
    public async Task Handle_WhenTypeIsPrivacy_CallsAcceptPrivacyPolicyOnAccount(string type)
    {
        //Arrange
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), type);
        Account account = TestDataFactory.CreateAccount();
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.AccountId, Ct))
            .ReturnsAsync(account);

        //Act
        await _handler.Handle(command, Ct);

        //Assert
        account.PrivacyPolicyAccepted.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_WhenAccountDoesNotExist_ThrowsAccountNotFoundException()
    {
        //Arrange
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), "Terms");
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.AccountId, Ct))
            .ReturnsAsync((Account?)null);

        //Act
        AccountNotFoundException exception = await Should.ThrowAsync<AccountNotFoundException>(
            () => _handler.Handle(command, Ct));

        //Assert
        exception.Message.ShouldBe($"Contul cu id-ul {command.AccountId} nu a fost gasit");
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Never);
        LoggerMock.VerifyLogError(command.AccountId.ToString(), Times.Once());
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("TermsAndConditions")]
    [InlineData("PrivacyPolicy")]
    [InlineData("123")]
    public async Task Handle_WhenInvalidType_ThrowsLegalDocInvalidTypeException(string type)
    {
        //Arrange
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), type);
        Account account = TestDataFactory.CreateAccount();
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.AccountId, Ct))
            .ReturnsAsync(account);

        //Act
        LegalDocInvalidTypeException exception = await Should.ThrowAsync<LegalDocInvalidTypeException>(
            () => _handler.Handle(command, Ct));

        //Assert
        exception.Message.ShouldBe("Invalid legal document type");
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Never);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenInvalidType_DoesNotModifyAccount()
    {
        //Arrange
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), "invalid");
        Account account = TestDataFactory.CreateAccount();
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(command.AccountId, Ct))
            .ReturnsAsync(account);

        //Act
        await Should.ThrowAsync<LegalDocInvalidTypeException>(() => _handler.Handle(command, Ct));

        //Assert
        account.TermsAccepted.ShouldBeFalse();
        account.PrivacyPolicyAccepted.ShouldBeFalse();
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public async Task Constructor_WhenDependenciesAreNull_ThrowsArgumentNullException(
        bool isRepoNull, bool isUnitOfWorkNull, bool isLoggerNull)
    {
        //Arrange
        var command = new AcceptLegalDocCommand(Guid.NewGuid(), "Terms");

        //Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() =>
            new AcceptLegalDocCommandHandler(
                isRepoNull ? null! : _accountRepositoryMock.Object,
                isUnitOfWorkNull ? null! : UnitOfWorkMock.Object,
                isLoggerNull ? null! : LoggerMock.Object)
            .Handle(command, Ct));
    }
}
