using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Integration.Setup;

[Collection("RepositoryCollection")]
public abstract class BaseRepositoryTest : IAsyncLifetime, IDisposable
{
    private Func<Task> ResetDatabase { get; }
    protected TestAppDbContext Context { get; }

    protected BaseRepositoryTest(IntegrationTestFixture fixture)
    {
        DbContextOptions<TestAppDbContext> options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .Options;

        Context = new TestAppDbContext(options);

        ResetDatabase = async () =>
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
        await ResetDatabase();
    }

    public async Task DisposeAsync() => await Context.DisposeAsync();

    protected static Account CreateAccount(string email) =>
        Account.Create(email, "Password1!", Guid.NewGuid().ToString("N")[..20], "Full Name");

    public void Dispose()
    {
        Context.Dispose();
        GC.SuppressFinalize(this);
    }
}
