using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareerFlow.Core.Api.IntegrationTests.Setup;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient AnonymousClient;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        AnonymousClient = Factory.CreateClient();
    }

    public virtual async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync() => Task.CompletedTask;

    protected async Task<(HttpClient Client, AccountDto Account, LoginRequest Credentials)>
        CreateAndAuthenticateUserAsync(
            string email = "test@email.com", string password = "testPassword")
    {
        await AnonymousClient.PostAsJsonAsync("/account/register",
            new CreateAccountRequest(email, password, "testUser", "Test Name"));

        var credentials = new LoginRequest(email, password);
        var loginResponse = await AnonymousClient.PostAsJsonAsync("/account/login", credentials);
        loginResponse.EnsureSuccessStatusCode();

        var account = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", account!.Token);

        return (client, account, credentials);
    }
}