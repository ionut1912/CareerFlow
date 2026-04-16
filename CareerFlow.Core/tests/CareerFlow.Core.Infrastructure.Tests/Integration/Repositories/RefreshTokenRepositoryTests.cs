using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Tests.Integration.Setup;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class RefreshTokenRepositoryTests : BaseRepositoryTest, IAsyncLifetime
{
    private readonly RefreshTokenRepository _sut;

    public RefreshTokenRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new RefreshTokenRepository(Context.RefreshTokens);
    }

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Context.RefreshTokens.RemoveRange(Context.RefreshTokens);
        Context.Accounts.RemoveRange(Context.Accounts);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task AddAsync_Token_PersistsToDatabase()
    {
        // Arrange
        var account = await SeedAccountAsync();
        var (token, rawToken) = CreateToken(account.Id);

        // Act
        await _sut.AddAsync(token, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.RefreshTokens.FindAsync(token.Id);
        saved.ShouldNotBeNull();
        saved!.UserId.ShouldBe(account.Id);
        saved.IsUsed.ShouldBeFalse();
        saved.IsRevoked.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsToken()
    {
        // Arrange
        var token = await SeedTokenAsync();

        // Act
        var result = await _sut.GetByIdAsync(token.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(token.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithAccountInclude_AccountIdIsSet()
    {
        // Arrange
        var account = await SeedAccountAsync();
        var token = await SeedTokenAsync(account.Id);

        // Act
        var result = await _sut.GetByIdAsync(token.Id, CancellationToken.None, t => t.Account);

        // Assert
        result.ShouldNotBeNull();
        result!.UserId.ShouldBe(account.Id);
    }

    [Fact]
    public async Task GetAllAsync_MultipleTokens_ReturnsAll()
    {
        // Arrange
        var account = await SeedAccountAsync();
        await SeedTokenAsync(account.Id);
        await SeedTokenAsync(account.Id);

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyTable_ReturnsEmptyCollection()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Delete_ExistingToken_RemovesFromDatabase()
    {
        // Arrange
        var token = await SeedTokenAsync();
        var tracked = await Context.RefreshTokens.FindAsync(token.Id);

        // Act
        _sut.Delete(tracked!);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var deleted = await Context.RefreshTokens.FindAsync(token.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Update_MarkAsUsed_IsPersisted()
    {
        // Arrange
        var token = await SeedTokenAsync();
        var tracked = await Context.RefreshTokens.FindAsync(token.Id);
        tracked!.MarkAsUsed();

        // Act
        _sut.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.RefreshTokens.FindAsync(token.Id);
        refreshed!.IsUsed.ShouldBeTrue();
        refreshed.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Update_MarkAsRevoked_IsPersisted()
    {
        // Arrange
        var token = await SeedTokenAsync();
        var tracked = await Context.RefreshTokens.FindAsync(token.Id);
        tracked!.MarkAsRevoked();

        // Act
        _sut.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.RefreshTokens.FindAsync(token.Id);
        refreshed!.IsRevoked.ShouldBeTrue();
        refreshed.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetExistingTokenAsync_MatchingRawToken_ReturnsToken()
    {
        // Arrange
        var account = await SeedAccountAsync();
        var rawToken = "my-secure-raw-token";
        var token = RefreshToken.Create(account.Id, rawToken, Guid.NewGuid().ToString(), DateTime.UtcNow.AddDays(7));
        Context.RefreshTokens.Add(token);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetExistingTokenAsync(token.TokenHash, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(token.Id);
    }

    [Fact]
    public async Task GetExistingTokenAsync_WrongToken_ReturnsNull()
    {
        // Arrange
        await SeedTokenAsync();

        // Act
        var result = await _sut.GetExistingTokenAsync("wrong-token-hash", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetExistingTokenAsync_EmptyTable_ReturnsNull()
    {
        // Act
        var result = await _sut.GetExistingTokenAsync("any-token-hash", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetExistingTokenAsync_MultipleTokens_ReturnsCorrectOne()
    {
        // Arrange
        var account = await SeedAccountAsync();
        var rawToken1 = "token-one";
        var rawToken2 = "token-two";
        var token1 = RefreshToken.Create(account.Id, rawToken1, Guid.NewGuid().ToString(), DateTime.UtcNow.AddDays(7));
        var token2 = RefreshToken.Create(account.Id, rawToken2, Guid.NewGuid().ToString(), DateTime.UtcNow.AddDays(7));
        Context.RefreshTokens.AddRange(token1, token2);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetExistingTokenAsync(token2.TokenHash, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(token2.Id);
    }

    private async Task<Account> SeedAccountAsync()
    {
        var account = Account.Create(
            $"{Guid.NewGuid():N}@test.com",
            "Password1!",
            Guid.NewGuid().ToString("N")[..12],
            "Full Name");
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return account;
    }

    private async Task<RefreshToken> SeedTokenAsync(Guid? userId = null)
    {
        if (userId is null)
        {
            var account = await SeedAccountAsync();
            userId = account.Id;
        }

        var (token, _) = CreateToken(userId.Value);
        Context.RefreshTokens.Add(token);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return token;
    }

    private static (RefreshToken token, string rawToken) CreateToken(Guid userId)
    {
        var rawToken = Guid.NewGuid().ToString();
        var token = RefreshToken.Create(userId, rawToken, Guid.NewGuid().ToString(), DateTime.UtcNow.AddDays(7));
        return (token, rawToken);
    }
}