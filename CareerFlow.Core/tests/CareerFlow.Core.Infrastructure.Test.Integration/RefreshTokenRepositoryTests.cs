using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Test.Integration.Setup;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Test.Integration;

public class RefreshTokenRepositoryTests : BaseRepositoryTest
{
    private readonly RefreshTokenRepository _sut;

    public RefreshTokenRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new RefreshTokenRepository(Context.RefreshTokens);
    }

    [Fact]
    public async Task GetExistingTokenAsync_MatchingJwtToken_ReturnsRefreshToken()
    {
        // Arrange
        var account = CreateAccount("rt@test.com");
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        var refreshToken = RefreshToken.Create(
            account.Id, "random-token-string", "jwt.token.here", DateTime.UtcNow.AddMonths(6));

        Context.RefreshTokens.Add(refreshToken);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetExistingTokenAsync("jwt.token.here", CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.JwtToken.ShouldBe("jwt.token.here");
    }

    [Fact]
    public async Task GetExistingTokenAsync_NoMatchingToken_ReturnsNull()
    {
        var result = await _sut.GetExistingTokenAsync("non.existent.jwt", CancellationToken.None);
        result.ShouldBeNull();
    }
}
