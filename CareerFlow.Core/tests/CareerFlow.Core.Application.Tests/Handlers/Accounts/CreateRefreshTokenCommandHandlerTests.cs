using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Application.CQRS.Accounts.Handler;
using CareerFlow.Core.Application.Tests.Common;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Models.Authentication;
using Moq;
using Shouldly;

namespace CareerFlow.Core.Application.Tests.Handlers.Accounts;

public class CreateRefreshTokenCommandHandlerTests : BaseHandlerTest<CreateRefreshTokenCommandHandler>
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly CreateRefreshTokenCommandHandler _handler;

    public CreateRefreshTokenCommandHandlerTests()
    {
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _tokenServiceMock = new Mock<ITokenService>();

        _handler = new CreateRefreshTokenCommandHandler(
            _loggerMock.Object,
            _accountRepositoryMock.Object,
            _tokenServiceMock.Object,
            _refreshTokenRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenStoredTokenValid_GeneratesNewToken()
    {
        // Arrange
        var command = new CreateRefreshTokenCommand("validToken", "validRefresh");
        var user = TestDataFactory.CreateAccount();
        var storedToken = TestDataFactory.CreateRefreshToken(user.Id);

        var newAuthResult = new AuthResult("newJwt", "expiry");
        var newRefreshToken = TestDataFactory.CreateRefreshToken(user.Id);

        _refreshTokenRepositoryMock.Setup(x => x.GetExistingTokenAsync(command.Token, Ct)).ReturnsAsync(storedToken);
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(user.Id, Ct)).ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateToken(user)).Returns(newAuthResult);
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken(user.Id, newAuthResult.Token)).Returns(newRefreshToken);

        // Act
        var result = await _handler.Handle(command, Ct);

        // Assert
        result.Token.ShouldBe(newAuthResult.Token);
        result.RefreshToken.ShouldBe(newRefreshToken.Token);
        _refreshTokenRepositoryMock.Verify(x => x.Update(storedToken), Times.Once);
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(newRefreshToken, Ct), Times.Once);
        _unitOfWorkMock.VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTokenNull_ThrowsInvalidRefreshTokenException()
    {
        // Arrange
        var command = new CreateRefreshTokenCommand("token", "refresh");
        _refreshTokenRepositoryMock.Setup(x => x.GetExistingTokenAsync(command.Token, Ct)).ReturnsAsync((RefreshToken?)null);

        // Act
        await Should.ThrowAsync<InvalidRefreshTokenException>(() => _handler.Handle(command, Ct));

        // Assert
        _loggerMock.VerifyLogError(command.RefreshToken, Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTokenUsed_ThrowsTokenAlreadyUsedException()
    {
        // Arrange
        var command = new CreateRefreshTokenCommand("token", "refresh");
        var storedToken = TestDataFactory.CreateRefreshToken(Guid.NewGuid());
        storedToken.MarkAsUsed();

        _refreshTokenRepositoryMock.Setup(x => x.GetExistingTokenAsync(command.Token, Ct)).ReturnsAsync(storedToken);

        // Act
        await Should.ThrowAsync<TokenAlreadyUsedExcception>(() => _handler.Handle(command, Ct));

        // Assert
        _loggerMock.VerifyLogError(command.RefreshToken, Times.Once());
    }

    [Fact]
    public async Task Handle_WhenTokenRevoked_ThrowsTokenRevokedException()
    {
        // Arrange
        var command = new CreateRefreshTokenCommand("token", "refresh");
        var storedToken = TestDataFactory.CreateRefreshToken(Guid.NewGuid());
        storedToken.MarkAsRevoked();

        _refreshTokenRepositoryMock.Setup(x => x.GetExistingTokenAsync(command.Token, Ct)).ReturnsAsync(storedToken);

        // Act
        await Should.ThrowAsync<TokenRevokedException>(() => _handler.Handle(command, Ct));

        // Assert
        _loggerMock.VerifyLogError(command.RefreshToken, Times.Once());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsAccountNotFoundException()
    {
        // Arrange
        var command = new CreateRefreshTokenCommand("token", "refresh");
        var storedToken = TestDataFactory.CreateRefreshToken(Guid.NewGuid());

        _refreshTokenRepositoryMock.Setup(x => x.GetExistingTokenAsync(command.Token, Ct)).ReturnsAsync(storedToken);
        _accountRepositoryMock.Setup(x => x.GetByIdAsync(storedToken.UserId, Ct)).ReturnsAsync((Account?)null);

        // Act
        await Should.ThrowAsync<AccountNotFoundException>(() => _handler.Handle(command, Ct));

        // Assert
        _loggerMock.VerifyLogError(storedToken.UserId.ToString(), Times.Once());
    }
}