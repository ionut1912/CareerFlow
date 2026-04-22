using CareerFlow.Core.Application.CQRS.Accounts.Handlers;
using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.Authentication;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Accounts;

public class LoginQueryHandlerTests : BaseHandlerTest<LoginQueryHandler>
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly LoginQueryHandler _handler;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;

    public LoginQueryHandlerTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _tokenServiceMock = new Mock<ITokenService>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();

        _handler = new LoginQueryHandler(
            _accountRepositoryMock.Object,
            _passwordServiceMock.Object,
            _tokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            UnitOfWorkMock.Object,
            LoggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCredentialsValid_ReturnsAuthResult()
    {
        // Arrange
        var account = TestDataFactory.CreateAccount();
        var query = new LoginQuery(account.Email, "password");
        var authResult = new AuthResult("token", "expiry");
        var refreshToken = TestDataFactory.CreateRefreshToken(account.Id);

        _accountRepositoryMock.Setup(x => x.GetAccountByEmailAsync(account.Email, Ct)).ReturnsAsync(account);
        _passwordServiceMock.Setup(x => x.VerifyPassword(query.Password, account.Password)).Returns(true);
        _tokenServiceMock.Setup(x => x.GenerateToken(account)).Returns(authResult);
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken(account.Id, authResult.Token)).Returns(refreshToken);

        // Act
        var result = await _handler.Handle(query, Ct);

        // Assert
        result.Token.ShouldBe(authResult.Token);
        result.RefreshToken.ShouldBe(refreshToken.TokenHash);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(refreshToken, Ct), Times.Once);
        UnitOfWorkMock.VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ThrowsAccountNotFoundException()
    {
        // Arrange
        var query = new LoginQuery("missing@mail.com", "pass");
        _accountRepositoryMock.Setup(x => x.GetAccountByEmailAsync(query.Email, Ct)).ReturnsAsync((Account?)null);

        // Act
        var exception = await Should.ThrowAsync<AccountNotFoundException>(() => _handler.Handle(query, Ct));

        // Assert
        LoggerMock.VerifyLogError(query.Email, Times.Once());
    }

    [Fact]
    public async Task Handle_WhenPasswordInvalid_ThrowsPasswordNotMatchException()
    {
        // Arrange
        var account = TestDataFactory.CreateAccount();
        var query = new LoginQuery(account.Email, "wrongpass");

        _accountRepositoryMock.Setup(x => x.GetAccountByEmailAsync(account.Email, Ct)).ReturnsAsync(account);
        _passwordServiceMock.Setup(x => x.VerifyPassword(query.Password, account.Password)).Returns(false);

        // Act
        await Should.ThrowAsync<PasswordNotMatchException>(() => _handler.Handle(query, Ct));

        // Assert
        LoggerMock.VerifyLogError("parola", Times.Once());
    }

    [Theory]
    [InlineData(true, false, false, false, false, false)]
    [InlineData(false, true, false, false, false, false)]
    [InlineData(false, false, true, false, false, false)]
    [InlineData(false, false, false, true, false, false)]
    [InlineData(false, false, false, false, true, false)]
    [InlineData(false, false, false, false, false, true)]
    public void Constructor_WhenDependenciesAreNull_ThrowsArgumentNullException(
        bool isRepoNull, bool isPasswordServiceNull, bool isTokenServiceNull,
        bool isRefreshTokenServiceNull, bool isUnitOfWorkNull, bool isLoggerNull)
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new LoginQueryHandler(
            isRepoNull ? null! : _accountRepositoryMock.Object,
            isPasswordServiceNull ? null! : _passwordServiceMock.Object,
            isTokenServiceNull ? null! : _tokenServiceMock.Object,
            isRefreshTokenServiceNull ? null! : _refreshTokenRepositoryMock.Object,
            isUnitOfWorkNull ? null! : UnitOfWorkMock.Object,
            isLoggerNull ? null! : LoggerMock.Object));
    }
}