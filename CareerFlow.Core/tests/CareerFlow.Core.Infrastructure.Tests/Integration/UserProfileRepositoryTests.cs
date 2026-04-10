using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.ValueObjects;
using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Test.Setup;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Test.Integration;

[Trait("Category", "Integration")]
public class UserProfileRepositoryIntegrationTests : BaseRepositoryTest
{
    private readonly UserProfileRepository _sut;
 
    public UserProfileRepositoryIntegrationTests(IntegrationTestFixture fixture) : base(fixture)
    {
        _sut = new UserProfileRepository(Context.UserProfiles);
    }
 
    private static UserProfile CreateProfile(Guid accountId)
        => UserProfile.Create(accountId, LearningType.Visual, [UserType.Student], "Tech");
 
    [Fact]
    public async Task GetCurrentUserProfile_ExistingAccount_ReturnsProfile()
    {
        var account = CreateAccount("profile@test.com");
        await Context.Accounts.AddAsync(account);
        var profile = CreateProfile(account.Id);
        await Context.UserProfiles.AddAsync(profile);
        await Context.SaveChangesAsync();
 
        var result = await _sut.GetCurrentUserProfile(account.Id, CancellationToken.None);
 
        result.ShouldNotBeNull();
        result.AccountId.ShouldBe(account.Id);
    }
 
    [Fact]
    public async Task GetCurrentUserProfile_NonExistingAccount_ReturnsNull()
    {
        var result = await _sut.GetCurrentUserProfile(Guid.NewGuid(), CancellationToken.None);
 
        result.ShouldBeNull();
    }
 
    [Fact]
    public async Task GetCurrentUserProfile_IncludesAccountNavigation()
    {
        var account = CreateAccount("nav@test.com");
        await Context.Accounts.AddAsync(account);
        var profile = CreateProfile(account.Id);
        await Context.UserProfiles.AddAsync(profile);
        await Context.SaveChangesAsync();
 
        var result = await _sut.GetCurrentUserProfile(account.Id, CancellationToken.None);
 
        result.ShouldNotBeNull();
        result.Account.ShouldNotBeNull();
        result.Account.Email.ShouldBe("nav@test.com");
    }
 
    [Fact]
    public async Task GetUserCourses_ExistingAccount_ReturnsProfile()
    {
        var account = CreateAccount("courses@test.com");
        await Context.Accounts.AddAsync(account);
        var profile = CreateProfile(account.Id);
        await Context.UserProfiles.AddAsync(profile);
        await Context.SaveChangesAsync();
 
        var result = await _sut.GetUserCourses(account.Id, CancellationToken.None);
 
        result.ShouldNotBeNull();
        result.AccountId.ShouldBe(account.Id);
    }
 
    [Fact]
    public async Task GetUserCourses_NonExistingAccount_ReturnsNull()
    {
        var result = await _sut.GetUserCourses(Guid.NewGuid(), CancellationToken.None);
 
        result.ShouldBeNull();
    }
 
    [Fact]
    public async Task AddAsync_ValidProfile_PersistsToDatabase()
    {
        var account = CreateAccount("addprofile@test.com");
        await Context.Accounts.AddAsync(account);
        await Context.SaveChangesAsync();
 
        var profile = CreateProfile(account.Id);
        await _sut.AddAsync(profile, CancellationToken.None);
        await Context.SaveChangesAsync();
 
        var persisted = await Context.UserProfiles.FirstOrDefaultAsync(p => p.AccountId == account.Id);
        persisted.ShouldNotBeNull();
    }
 
    [Fact]
    public async Task Update_ExistingProfile_PersistsDomainChange()
    {
        var account = CreateAccount("updateprofile@test.com");
        await Context.Accounts.AddAsync(account);
        var profile = CreateProfile(account.Id);
        await Context.UserProfiles.AddAsync(profile);
        await Context.SaveChangesAsync();
 
        profile.Update(LearningType.Auditory, [UserType.JobSearcher], "Medicine");
        _sut.Update(profile);
        await Context.SaveChangesAsync();
        Context.ChangeTracker.Clear();
 
        var refreshed = await Context.UserProfiles.FirstAsync(p => p.AccountId == account.Id);
        refreshed.Domain.ShouldBe("Medicine");
    }
 
    [Fact]
    public async Task Delete_ExistingProfile_RemovesFromDatabase()
    {
        var account = CreateAccount("deleteprofile@test.com");
        await Context.Accounts.AddAsync(account);
        var profile = CreateProfile(account.Id);
        await Context.UserProfiles.AddAsync(profile);
        await Context.SaveChangesAsync();
 
        _sut.Delete(profile);
        await Context.SaveChangesAsync();
 
        var found = await Context.UserProfiles.FirstOrDefaultAsync(p => p.AccountId == account.Id);
        found.ShouldBeNull();
    }
}
 