using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistence.Repositories;
using CareerFlow.Core.Infrastructure.Tests.Integration.Setup;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Integration.Repositories;

[Trait("Category", "Integration")]
public class AccountRepositoryTests : BaseRepositoryTest, IAsyncLifetime
{
    private readonly AccountRepository _sut;

    public AccountRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new AccountRepository(Context.Accounts);
    }

    public new async Task InitializeAsync()
    {
        await base.InitializeAsync();

        Context.Accounts.RemoveRange(Context.Accounts);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
    }

    [Fact]
    public async Task AddAsync_Account_PersistsToDatabase()
    {
        // Arrange
        var account = CreateAccount("add@test.com");

        // Act
        await _sut.AddAsync(account, CancellationToken.None);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var saved = await Context.Accounts.FindAsync(account.Id);
        saved.ShouldNotBeNull();
        saved!.Email.ShouldBe("add@test.com");
        saved.TermsAccepted.ShouldBeFalse();
        saved.PrivacyPolicyAccepted.ShouldBeFalse();
        saved.IsFounder.ShouldBeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsAccount()
    {
        // Arrange
        var account = await SeedAccountAsync("byid@test.com");

        // Act
        var result = await _sut.GetByIdAsync(account.Id);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(account.Id);
        result.Email.ShouldBe("byid@test.com");
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
    public async Task GetAllAsync_MultipleAccounts_ReturnsAll()
    {
        // Arrange
        await SeedAccountAsync("first@test.com");
        await SeedAccountAsync("second@test.com");

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(a => a.Email == "first@test.com");
        result.ShouldContain(a => a.Email == "second@test.com");
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
    public async Task Delete_ExistingAccount_RemovesFromDatabase()
    {
        // Arrange
        var account = await SeedAccountAsync("delete@test.com");
        var tracked = await Context.Accounts.FindAsync(account.Id);

        // Act
        _sut.Delete(tracked!);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var deleted = await Context.Accounts.FindAsync(account.Id);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task Update_AcceptTerms_IsPersisted()
    {
        // Arrange
        var account = await SeedAccountAsync("terms@test.com");
        var tracked = await Context.Accounts.FindAsync(account.Id);
        tracked!.AcceptTerms();

        // Act
        _sut.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.Accounts.FindAsync(account.Id);
        refreshed!.TermsAccepted.ShouldBeTrue();
        refreshed.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Update_AcceptPrivacyPolicy_IsPersisted()
    {
        // Arrange
        var account = await SeedAccountAsync("privacy@test.com");
        var tracked = await Context.Accounts.FindAsync(account.Id);
        tracked!.AcceptPrivacyPolicy();

        // Act
        _sut.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.Accounts.FindAsync(account.Id);
        refreshed!.PrivacyPolicyAccepted.ShouldBeTrue();
        refreshed.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Update_MarkAsFounder_IsPersisted()
    {
        // Arrange
        var account = await SeedAccountAsync("founder@test.com");
        var tracked = await Context.Accounts.FindAsync(account.Id);
        tracked!.MarkAsFounder();

        // Act
        _sut.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.Accounts.FindAsync(account.Id);
        refreshed!.IsFounder.ShouldBeTrue();
        refreshed.UpdatedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAccountByEmailAsync_ExistingEmail_ReturnsAccount()
    {
        // Arrange
        await SeedAccountAsync("match@test.com");

        // Act
        var result = await _sut.GetAccountByEmailAsync("match@test.com", CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.Email.ShouldBe("match@test.com");
    }

    [Fact]
    public async Task GetAccountByEmailAsync_NonExistentEmail_ReturnsNull()
    {
        // Act
        var result = await _sut.GetAccountByEmailAsync("nobody@test.com", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAccountByEmailAsync_EmailIsCaseSensitive_ReturnsNull()
    {
        // Arrange
        await SeedAccountAsync("case@test.com");

        // Act
        var result = await _sut.GetAccountByEmailAsync("CASE@TEST.COM", CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAccountByEmailAsync_MultipleAccounts_ReturnsCorrectOne()
    {
        // Arrange
        await SeedAccountAsync("first@test.com");
        await SeedAccountAsync("second@test.com");

        // Act
        var result = await _sut.GetAccountByEmailAsync("second@test.com", CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.Email.ShouldBe("second@test.com");
    }

    [Fact]
    public async Task UpdateTermsAsync_Terms_SetsTermsAcceptedFalseForAllAccounts()
    {
        // Arrange
        var account1 = await SeedAccountAsync("t1@test.com");
        var account2 = await SeedAccountAsync("t2@test.com");
        var tracked1 = await Context.Accounts.FindAsync(account1.Id);
        var tracked2 = await Context.Accounts.FindAsync(account2.Id);
        tracked1!.AcceptTerms();
        tracked2!.AcceptTerms();
        Context.Accounts.UpdateRange(tracked1, tracked2);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        await _sut.UpdateTermsAsync("Terms", CancellationToken.None);
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed1 = await Context.Accounts.FindAsync(account1.Id);
        var refreshed2 = await Context.Accounts.FindAsync(account2.Id);
        refreshed1!.TermsAccepted.ShouldBeFalse();
        refreshed2!.TermsAccepted.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateTermsAsync_Privacy_SetsPrivacyPolicyAcceptedFalseForAllAccounts()
    {
        // Arrange
        var account1 = await SeedAccountAsync("p1@test.com");
        var account2 = await SeedAccountAsync("p2@test.com");
        var tracked1 = await Context.Accounts.FindAsync(account1.Id);
        var tracked2 = await Context.Accounts.FindAsync(account2.Id);
        tracked1!.AcceptPrivacyPolicy();
        tracked2!.AcceptPrivacyPolicy();
        Context.Accounts.UpdateRange(tracked1, tracked2);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        await _sut.UpdateTermsAsync("Privacy", CancellationToken.None);
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed1 = await Context.Accounts.FindAsync(account1.Id);
        var refreshed2 = await Context.Accounts.FindAsync(account2.Id);
        refreshed1!.PrivacyPolicyAccepted.ShouldBeFalse();
        refreshed2!.PrivacyPolicyAccepted.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateTermsAsync_Terms_DoesNotAffectPrivacyPolicy()
    {
        // Arrange
        var account = await SeedAccountAsync("notaffected@test.com");
        var tracked = await Context.Accounts.FindAsync(account.Id);
        tracked!.AcceptPrivacyPolicy();
        Context.Accounts.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        await _sut.UpdateTermsAsync("Terms", CancellationToken.None);
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.Accounts.FindAsync(account.Id);
        refreshed!.PrivacyPolicyAccepted.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateTermsAsync_Privacy_DoesNotAffectTermsAccepted()
    {
        // Arrange
        var account = await SeedAccountAsync("notaffected2@test.com");
        var tracked = await Context.Accounts.FindAsync(account.Id);
        tracked!.AcceptTerms();
        Context.Accounts.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        await _sut.UpdateTermsAsync("Privacy", CancellationToken.None);
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.Accounts.FindAsync(account.Id);
        refreshed!.TermsAccepted.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateTermsAsync_UnknownDocumentType_ChangesNothing()
    {
        // Arrange
        var account = await SeedAccountAsync("unchanged@test.com");
        var tracked = await Context.Accounts.FindAsync(account.Id);
        tracked!.AcceptTerms();
        tracked.AcceptPrivacyPolicy();
        Context.Accounts.Update(tracked);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        await _sut.UpdateTermsAsync("Unknown", CancellationToken.None);
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.Accounts.FindAsync(account.Id);
        refreshed!.TermsAccepted.ShouldBeTrue();
        refreshed.PrivacyPolicyAccepted.ShouldBeTrue();
    }

    private async Task<Account> SeedAccountAsync(string email)
    {
        var account = CreateAccount(email);
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return account;
    }
}