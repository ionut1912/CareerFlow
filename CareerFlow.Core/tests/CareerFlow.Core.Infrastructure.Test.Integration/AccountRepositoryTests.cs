using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Test.Integration.Setup;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Test.Integration;

public class AccountRepositoryTests : BaseRepositoryTest
{
    private readonly AccountRepository _sut;

    public AccountRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new AccountRepository(Context.Accounts);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsEntity()
    {
        // Arrange
        var account = CreateAccount("byid@test.com");
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetByIdAsync(account.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(account.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAccountByEmailAsync_ExistingEmail_ReturnsAccount()
    {
        // Arrange
        var account = CreateAccount("match@test.com");
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAccountByEmailAsync("match@test.com", CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Email.ShouldBe("match@test.com");
    }

    [Fact]
    public async Task ResetLegalAgreementsAsync_MultipleAccounts_SetsAllAgreementsToFalse()
    {
        // Arrange
        var a1 = CreateAccount("reset1@test.com");
        var a2 = CreateAccount("reset2@test.com");
        a1.AcceptTerms();
        a1.AcceptPrivacyPolicy();
        a2.AcceptTerms();
        a2.AcceptPrivacyPolicy();

        Context.Accounts.AddRange(a1, a2);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        await _sut.ResetLegalAgreementsAsync(CancellationToken.None);
        Context.ChangeTracker.Clear();

        // Assert
        var updated = await Context.Accounts.ToListAsync();
        updated.ShouldAllBe(a => !a.PrivacyPolicyAccepted);
        updated.ShouldAllBe(a => !a.TermsAccepted);
    }

    [Fact]
    public async Task AddAsync_EntityAppearsInGetAllAsync()
    {
        // Arrange
        var account = CreateAccount("add@test.com");

        // Act
        await _sut.AddAsync(account);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var all = (await _sut.GetAllAsync()).ToList();

        // Assert
        all.ShouldContain(a => a.Email == "add@test.com");
    }

    [Fact]
    public async Task Update_ChangesArePersistedToDb()
    {
        // Arrange
        var account = CreateAccount("upd@test.com");
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var loaded = (await Context.Accounts.FindAsync(account.Id))!;
        loaded.AcceptTerms();

        // Act
        _sut.Update(loaded);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var refreshed = await Context.Accounts.FindAsync(account.Id);
        refreshed!.TermsAccepted.ShouldBeTrue();
    }
}