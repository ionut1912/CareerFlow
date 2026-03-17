using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;
using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Test.Integration.Setup;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Test.Integration;

[Trait("Category", "Integration")]
public sealed class UserProfileRepositoryTests : BaseRepositoryTest
{
    private readonly UserProfileRepository _sut;

    public UserProfileRepositoryTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new UserProfileRepository(Context.UserProfiles);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<Account> SeedAccountAsync(string email = "user@test.com")
    {
        var account = CreateAccount(email);
        await Context.Accounts.AddAsync(account);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
        return account;
    }

    private static UserProfile BuildProfile(
        Guid accountId,
        string domain = "Engineering",
        string learningType = "Visual",
        string[]? userTypes = null)
    {
        var types = (userTypes ?? ["JobSearcher"])
            .Select(UserType.FromString)
            .ToList();

        return UserProfile.Create(
            accountId,
            LearningType.FromString(learningType),
            types,
            domain);
    }

    // ════════════════════════════════════════════════════════════════════════
    // AddAsync
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task AddAsync_ValidProfile_PersistsToDatabase()
    {
        // Arrange
        var account = await SeedAccountAsync();
        var profile = BuildProfile(account.Id);

        // Act
        await _sut.AddAsync(profile);
        await Context.SaveChangesAsync();

        // Assert
        var saved = await Context.UserProfiles.FindAsync(profile.Id);
        saved.ShouldNotBeNull();
        saved.AccountId.ShouldBe(account.Id);
        saved.Domain.ShouldBe("Engineering");
        saved.CorrectAnswersForQuiz.ShouldBe(0);
        saved.IncorrectAnswersForQuiz.ShouldBe(0);
        saved.Experience.ShouldBe(0);
    }

    [Fact]
    public async Task AddAsync_TwoProfilesForSameAccount_ThrowsDueToUniqueIndexViolation()
    {
        // Arrange
        var account = await SeedAccountAsync();
        await _sut.AddAsync(BuildProfile(account.Id));
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        await _sut.AddAsync(BuildProfile(account.Id)); // same AccountId — violates unique index

        // Act & Assert
        await Should.ThrowAsync<Exception>(async () => await Context.SaveChangesAsync());
    }

    // ════════════════════════════════════════════════════════════════════════
    // GetByIdAsync
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProfile()
    {
        // Arrange
        var account = await SeedAccountAsync();
        var profile = BuildProfile(account.Id);
        await _sut.AddAsync(profile);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetByIdAsync(profile.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(profile.Id);
        result.AccountId.ShouldBe(account.Id);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithUserTypesInclude_LoadsOwnedCollection()
    {
        // Arrange
        var account = await SeedAccountAsync();
        var profile = BuildProfile(account.Id, userTypes: ["JobSearcher", "Student"]);
        await _sut.AddAsync(profile);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetByIdAsync(profile.Id, includes: up => up.UserTypes);

        // Assert
        result.ShouldNotBeNull();
        result.UserTypes.Count.ShouldBe(2);
        result.UserTypes.Select(ut => ut.Value).ShouldBe(["JobSearcher", "Student"], true);
    }

    // ════════════════════════════════════════════════════════════════════════
    // GetAllAsync
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetAllAsync_WhenTableIsEmpty_ReturnsEmptyCollection()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_MultipleProfiles_ReturnsAll()
    {
        // Arrange
        var account1 = await SeedAccountAsync("a1@test.com");
        var account2 = await SeedAccountAsync("a2@test.com");
        await _sut.AddAsync(BuildProfile(account1.Id, "Finance"));
        await _sut.AddAsync(BuildProfile(account2.Id));
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var result = (await _sut.GetAllAsync()).ToList();

        // Assert
        result.Count.ShouldBe(2);
        result.Select(p => p.Domain).ShouldBe(["Finance", "Engineering"], true);
    }

    // ════════════════════════════════════════════════════════════════════════
    // Update
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Update_ChangingDomainLearningTypeAndUserTypes_PersistsAllChanges()
    {
        // Arrange
        var account = await SeedAccountAsync();
        var profile = BuildProfile(account.Id, "Finance", "Visual", ["JobSearcher"]);
        await _sut.AddAsync(profile);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var toUpdate = await Context.UserProfiles.FindAsync(profile.Id);
        toUpdate!.Update(
            LearningType.FromString("Auditory"),
            [UserType.FromString("Student")],
            "Healthcare");

        // Act
        _sut.Update(toUpdate);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Assert
        var updated = await Context.UserProfiles.FindAsync(profile.Id);
        updated.ShouldNotBeNull();
        updated.Domain.ShouldBe("Healthcare");
        updated.LearningType.Value.ShouldBe("Auditory");
    }

    // ════════════════════════════════════════════════════════════════════════
    // Delete
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Delete_ExistingProfile_RemovesFromDatabase()
    {
        // Arrange
        var account = await SeedAccountAsync();
        var profile = BuildProfile(account.Id);
        await _sut.AddAsync(profile);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        var toDelete = await Context.UserProfiles.FindAsync(profile.Id);

        // Act
        _sut.Delete(toDelete!);
        await Context.SaveChangesAsync();

        // Assert
        var deleted = await Context.UserProfiles.FindAsync(profile.Id);
        deleted.ShouldBeNull();
    }

    // ════════════════════════════════════════════════════════════════════════
    // GetCurrentUserProfile
    // ════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetCurrentUserProfile_ExistingAccountId_ReturnsProfileWithAccountAndUserTypesLoaded()
    {
        // Arrange
        var account = await SeedAccountAsync("loaded@test.com");
        var profile = BuildProfile(
            account.Id,
            "Data Science",
            "Auditory",
            ["JobSearcher", "Student"]);

        await _sut.AddAsync(profile);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetCurrentUserProfile(account.Id, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(profile.Id);
        result.Domain.ShouldBe("Data Science");

        result.Account.ShouldNotBeNull();
        result.Account.Email.ShouldBe("loaded@test.com");

        result.UserTypes.Count.ShouldBe(2);
        result.UserTypes.Select(ut => ut.Value).ShouldBe(["JobSearcher", "Student"], true);

        result.LearningType.Value.ShouldBe("Auditory");
    }

    [Fact]
    public async Task GetCurrentUserProfile_UnknownAccountId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetCurrentUserProfile(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetCurrentUserProfile_WhenMultipleProfilesExist_ReturnsOnlyTheCorrectOne()
    {
        // Arrange
        var account1 = await SeedAccountAsync("acc1@test.com");
        var account2 = await SeedAccountAsync("acc2@test.com");

        await _sut.AddAsync(BuildProfile(account1.Id, "Finance"));
        await _sut.AddAsync(BuildProfile(account2.Id));
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();

        // Act
        var result = await _sut.GetCurrentUserProfile(account2.Id, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.AccountId.ShouldBe(account2.Id);
        result.Domain.ShouldBe("Engineering");
    }

    [Fact]
    public async Task GetCurrentUserProfile_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _sut.GetCurrentUserProfile(Guid.NewGuid(), cts.Token));
    }
}