using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Test.Integration.Setup;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Test.Integration;

[Trait("Category", "Integration")]
public class RefreshTokenRepositoryTests : BaseRepositoryTest
{
    private readonly RefreshTokenRepository _sut;

    public RefreshTokenRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new RefreshTokenRepository(Context.RefreshTokens);
    }

    [Fact]
    public async Task GetExistingTokenAsync_MatchingRefreshToken_ReturnsRefreshToken()
    {
        //Arrange
        var account = CreateAccount("rt@test.com");
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        var refreshToken = RefreshToken.Create(
            account.Id, "random-token-string", "jwt.token.here", DateTime.UtcNow.AddMonths(6));

        Context.RefreshTokens.Add(refreshToken);
        await Context.SaveChangesAsync();

        //Act
        var result = await _sut.GetExistingTokenAsync("random-token-string", CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.Token.ShouldBe("random-token-string");
    }

    [Fact]
    public async Task GetExistingTokenAsync_NoMatchingToken_ReturnsNull()
    {
        //Act
        var result = await _sut.GetExistingTokenAsync("non.existent.refresh", CancellationToken.None);

        //Assert
        result.ShouldBeNull();
    }
}