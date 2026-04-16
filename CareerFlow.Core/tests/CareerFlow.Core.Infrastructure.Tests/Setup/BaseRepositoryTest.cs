using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Setup;

[Collection("RepositoryCollection")]
public abstract class BaseRepositoryTest : IAsyncLifetime
{
    private readonly Func<Task> _resetDatabase;
    protected readonly TestAppDbContext Context;

    protected BaseRepositoryTest(IntegrationTestFixture fixture)
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;

        Context = new TestAppDbContext(options);

        _resetDatabase = async () =>
        {
            // Delete in FK-safe order: children before parents
            Context.UserProfiles.RemoveRange(Context.UserProfiles);
            Context.RefreshTokens.RemoveRange(Context.RefreshTokens);
            Context.Accounts.RemoveRange(Context.Accounts);
            await Context.SaveChangesAsync();
            Context.ChangeTracker.Clear();
        };
    }

    public async Task InitializeAsync()
    {
        await Context.Database.EnsureCreatedAsync();
        await _resetDatabase();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
    }

    protected static Account CreateAccount(string email)
    {
        return Account.Create(email, "Password1!", Guid.NewGuid().ToString("N")[..20], "Full Name");
    }
}