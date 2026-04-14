using CareerFlow.Core.Application.CQRS.Accounts.Commands;
using CareerFlow.Core.Application.CQRS.Accounts.Handlers;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Accounts;

public class CreateAccountCommandHandlerTests : BaseHandlerTest<CreateAccountCommandHandler>
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly CreateAccountCommandHandler _handler;
    private readonly Mock<IPasswordService> _passwordServiceMock;

    public CreateAccountCommandHandlerTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _handler = new CreateAccountCommandHandler(
            _accountRepositoryMock.Object,
            _passwordServiceMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenEmailIsUnique_CreatesAccount()
    {
        // Arrange
        var command = new CreateAccountCommand("new@email.com", "pass", "pass", "user", "name");
        _accountRepositoryMock.Setup(x => x.GetAccountByEmailAsync(command.Email, Ct))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _handler.Handle(command, Ct);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        _passwordServiceMock.Verify(x => x.HashPassword(command.Password), Times.Once);
        _accountRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Account>(), Ct), Times.Once);
        _unitOfWorkMock.VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenEmailExists_ThrowsUserAlreadyExistsException()
    {
        // Arrange
        var command = new CreateAccountCommand("exist@email.com", "pass", "pass", "user", "name");
        var existingAccount = TestDataFactory.CreateAccount();

        _accountRepositoryMock.Setup(x => x.GetAccountByEmailAsync(command.Email, Ct))
            .ReturnsAsync(existingAccount);

        // Act
        var exception = await Should.ThrowAsync<UserAlreadyExistsException>(() => _handler.Handle(command, Ct));

        // Assert
        exception.Message.ShouldContain(command.Email);
        _loggerMock.VerifyLogError(command.Email, Times.Once());
        _unitOfWorkMock.VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task Handle_WhenPasswordsAreDifferent_ThrowsPasswordNotMatchException()
    {
        // Arrange
        var command = new CreateAccountCommand("new@email.com", "pass", "pass2", "user", "name");
        _accountRepositoryMock.Setup(x => x.GetAccountByEmailAsync(command.Email, Ct))
            .ReturnsAsync((Account?)null);

        // Act
        var exception = await Should.ThrowAsync<PasswordNotMatchException>(() => _handler.Handle(command, Ct));

        // Assert
        exception.Message.ShouldBe("Parolele nu corespund");
        _loggerMock.VerifyLogError("nu corespund", Times.Once());
        _unitOfWorkMock.VerifySaveChanges(Times.Never());
    }

    [Theory]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    [InlineData(true, true, true, true)]
    public async Task Handle_WhenDependenciesAreNull_ThrowsArgumentNullException(
        bool isRepoNull, bool isPasswordServiceNull, bool isUowNull, bool isLoggerNull)
    {
        // Arrange
        var command = new CreateAccountCommand("new@email.com", "pass", "pass", "user", "name");

        var repo = isRepoNull ? null! : _accountRepositoryMock.Object;
        var passwordService = isPasswordServiceNull ? null! : _passwordServiceMock.Object;
        var uow = isUowNull ? null! : _unitOfWorkMock.Object;
        var logger = isLoggerNull ? null! : _loggerMock.Object;

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
        {
            var handler = new CreateAccountCommandHandler(repo, passwordService, uow, logger);
            await handler.Handle(command, Ct);
        });
    }
}