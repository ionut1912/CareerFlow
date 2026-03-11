using System.Net.Http.Headers;
using System.Net.Http.Json;
using CareerFlow.Core.Api.IntegrationTests.Setup;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using CareerFlow.Core.Domain.Entities;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CareerFlow.Core.Api.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    protected readonly HttpClient AnonymousClient;
    protected readonly TestWebApplicationFactory Factory;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        AnonymousClient = Factory.CreateClient();
    }

    public virtual async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
       AnonymousClient?.Dispose();
        return Task.CompletedTask;
    }

    protected HttpClient CreateClientForNonExistentUser(Account account)
    {
        return Factory.CreateAuthenticatedClient(account);
    }

    protected async Task<(HttpClient Client, AccountDto Account, LoginRequest Credentials)>
        CreateAndAuthenticateUserAsync(string? email = null, string password = "testPassword")
    {
        var targetEmail = email ?? $"test_{Guid.NewGuid():N}@email.com";
        var targetUsername = $"user_{Guid.NewGuid():N}";

        var registerResponse = await AnonymousClient.PostAsJsonAsync("/account/register",
            new CreateAccountRequest(targetEmail, password, password, targetUsername, "Test Name"));

        if (!registerResponse.IsSuccessStatusCode)
        {
            var errorBody = await registerResponse.Content.ReadAsStringAsync();
            throw new Exception($"Register failed with {registerResponse.StatusCode}. Body: {errorBody}");
        }

        var credentials = new LoginRequest(targetEmail, password);
        var loginResponse = await AnonymousClient.PostAsJsonAsync("/account/login", credentials);

        if (!loginResponse.IsSuccessStatusCode)
        {
            var errorBody = await loginResponse.Content.ReadAsStringAsync();
            throw new Exception($"Login failed with {loginResponse.StatusCode}. Body: {errorBody}");
        }

        var account = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();

        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", account!.Token);

        return (client, account, credentials);
    }
}