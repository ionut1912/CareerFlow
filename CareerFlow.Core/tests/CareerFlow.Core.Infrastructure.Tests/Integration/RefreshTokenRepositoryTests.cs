using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Test.Setup;
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
    public async Task GetExistingTokenAsync_NoMatchingToken_ReturnsNull()
    {
        //Act
        var result = await _sut.GetExistingTokenAsync("non.existent.refresh", CancellationToken.None);

        //Assert
        result.ShouldBeNull();
    }
}