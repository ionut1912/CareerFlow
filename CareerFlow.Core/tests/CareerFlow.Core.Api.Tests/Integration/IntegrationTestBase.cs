using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareerFlow.Core.Api.Tests.Setup;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests.Account;
using CareerFlow.Core.Domain.Entities;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CareerFlow.Core.Api.Tests.Integration;

public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    protected HttpClient AnonymousClient { get; }
    protected TestWebApplicationFactory Factory { get; }

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        AnonymousClient = Factory.CreateClient();
    }

    public virtual async Task InitializeAsync() => await Factory.ResetDatabaseAsync();

    public virtual Task DisposeAsync()
    {
        AnonymousClient.Dispose();
        return Task.CompletedTask;
    }

    protected HttpClient CreateClientForNonExistentUser(Account account) => Factory.CreateAuthenticatedClient(account);

    protected async Task<(HttpClient Client, AccountDto Account, LoginRequest Credentials)>
        CreateAndAuthenticateUserAsync(string? email = null, string password = "testPassword")
    {
        string targetEmail = email ?? $"test_{Guid.NewGuid():N}@email.com";
        string targetUsername = $"user_{Guid.NewGuid():N}";

        HttpResponseMessage registerResponse = await AnonymousClient.PostAsJsonAsync("/account/register",
            new CreateAccountRequest(targetEmail, password, password, targetUsername, "Test Name"));

        if (!registerResponse.IsSuccessStatusCode)
        {
            string errorBody = await registerResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Register failed with {registerResponse.StatusCode}. Body: {errorBody}");
        }

        var credentials = new LoginRequest(targetEmail, password);
        HttpResponseMessage loginResponse = await AnonymousClient.PostAsJsonAsync("/account/login", credentials);

        if (!loginResponse.IsSuccessStatusCode)
        {
            string errorBody = await loginResponse.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Login failed with {loginResponse.StatusCode}. Body: {errorBody}");
        }

        AccountDto? account = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();

        HttpClient client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", account!.Token);

        return (client, account, credentials);
    }
}
