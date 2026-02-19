using System.Net;
using System.Net.Http.Json;
using CareerFlow.Core.Api.IntegrationTests.Setup;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests;

public class AccountEndpointsTests : IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    public AccountEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();

        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/account/register",
            new CreateAccountRequest("testEmail@email.com", "testPassword", "testUsername", "testName"));
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Register_ShouldReturnSuccess_WhenDataIsValid()
    {
        var client = _factory.CreateClient();
        var request = new CreateAccountRequest("newEmail@email.com", "testPassword", "newUsername", "testName");

        var response = await client.PostAsJsonAsync("/account/register", request);

        var result = await response.Content.ReadFromJsonAsync<Guid>();
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenDataIsInvalid()
    {
        var client = _factory.CreateClient();
        var request = new CreateAccountRequest("testEmail", "", "testUsername", "testName");

        var response = await client.PostAsJsonAsync("/account/register", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnNotFound_WhenUrlIsInvalid()
    {
        var client = _factory.CreateClient();
        var request = new CreateAccountRequest("testEmail", "testPassword", "testUsername", "testName");

        var response = await client.PostAsJsonAsync("/invalid-ur;", request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenAccountExists()
    {
        var client = _factory.CreateClient();
        var request = new CreateAccountRequest("testEmail@email.com", "testPassword", "testUsername", "testName");

        var response = await client.PostAsJsonAsync("/account/register", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        var client = _factory.CreateClient();
        var request = new LoginRequest("testEmail", "testPassword");

        var response = await client.PostAsJsonAsync("/account/login", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Return200_WhenDataIsValid()
    {
        var client = _factory.CreateClient();
        var request = new LoginRequest("testEmail@email.com", "testPassword");

        var response = await client.PostAsJsonAsync("/account/login", request);

        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AccountDto>();
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Email.ShouldBe(request.Email);
        result.Username.ShouldNotBe(null);
        result.RefreshToken.ShouldNotBe(null);
        result.IsFounder.ShouldBeFalse();
        result.PrivacyPolicyAccepted.ShouldBeTrue();
        result.TermsAccepted.ShouldBeTrue();
    }

    [Fact]
    public async Task Login_ShouldReturnNotFound_WhenUrlIsInvalid()
    {
        var client = _factory.CreateClient();
        var request = new LoginRequest("testEmail", "testPassword");

        var response = await client.PostAsJsonAsync("/invalid-uri", request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsInvalid()
    {
        var client = _factory.CreateClient();
        var request = new LoginRequest("testEmail@email.com", "testPassword2");

        var response = await client.PostAsJsonAsync("/account/login", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnNotFound_WhenEmailDoesNotExist()
    {
        var client = _factory.CreateClient();
        var request = new LoginRequest("testEmail2@email.com", "testPassword2");

        var response = await client.PostAsJsonAsync("/account/login", request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenIsNotAuthenticated()
    {
        var client = _factory.CreateClient();
        var request = new RefreshTokenRequest("testRefresh", "testRefreshToken");

        var response = await client.PostAsJsonAsync("/account/refresh-token", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnOk_WhenDataIsValid()
    {
        var anonClient = _factory.CreateClient();
        var request = new LoginRequest("testEmail@email.com", "testPassword");
        var loginResponse = await anonClient.PostAsJsonAsync("/account/login", request);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();
        loginResult.ShouldNotBeNull();
        var authClient = _factory.CreateAuthenticatedClient(loginResult.Id);
        var refreshTokenRequest = new RefreshTokenRequest(loginResult.Token, loginResult.RefreshToken);

        var response = await authClient.PostAsJsonAsync("/account/refresh-token", refreshTokenRequest);

        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RefreshTokenDto>();
        result.ShouldNotBeNull();
        result.RefreshToken.ShouldNotBeNull();
        result.Token.ShouldNotBeNull();
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnNotFound_WhenUrlIsInvalid()
    {
        var anonClient = _factory.CreateClient();
        var request = new LoginRequest("testEmail@email.com", "testPassword");
        var loginResponse = await anonClient.PostAsJsonAsync("/account/login", request);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();
        loginResult.ShouldNotBeNull();
        var authClient = _factory.CreateAuthenticatedClient(loginResult.Id);
        var refreshTokenRequest = new RefreshTokenRequest(loginResult.Token, loginResult.RefreshToken);

        var response = await authClient.PostAsJsonAsync("/invalidurl", refreshTokenRequest);

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnUnauthorized_WhenIsNotAuthenticated()
    {
        var client = _factory.CreateClient();
        var request = new ResetPasswordRequest("newPassword");

        var response = await client.PutAsJsonAsync("/account/reset-password", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenLoginWithOldPassword()
    {
        var anonClient = _factory.CreateClient();
        var request = new LoginRequest("testEmail@email.com", "testPassword");
        var loginResponse = await anonClient.PostAsJsonAsync("/account/login", request);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();
        loginResult.ShouldNotBeNull();
        var authClient = _factory.CreateAuthenticatedClient(loginResult.Id);
        var resetPasswordRequest = new ResetPasswordRequest("newPassword");

        var response = await authClient.PutAsJsonAsync("/account/reset-password", resetPasswordRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var reloginRequest = await anonClient.PostAsJsonAsync("/account/login", request);
        reloginRequest.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnSuccess_WhenLoginWithNewPassword()
    {
        var anonClient = _factory.CreateClient();
        var request = new LoginRequest("testEmail@email.com", "testPassword");
        var loginResponse = await anonClient.PostAsJsonAsync("/account/login", request);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();
        loginResult.ShouldNotBeNull();
        var authClient = _factory.CreateAuthenticatedClient(loginResult.Id);
        var resetPasswordRequest = new ResetPasswordRequest("newPassword");

        var response = await authClient.PutAsJsonAsync("/account/reset-password", resetPasswordRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var newLoginRequest = new LoginRequest(request.Email, resetPasswordRequest.NewPassword);
        var reloginRequest = await anonClient.PostAsJsonAsync("/account/login", newLoginRequest);
        reloginRequest.EnsureSuccessStatusCode();
        reloginRequest.StatusCode.ShouldBe(HttpStatusCode.OK);
        var reloginResult = await reloginRequest.Content.ReadFromJsonAsync<AccountDto>();
        reloginResult.ShouldNotBeNull();
        reloginResult.Id.ShouldNotBe(Guid.Empty);
        reloginResult.Email.ShouldNotBeNull();
        reloginResult.Username.ShouldNotBeNull();
        reloginResult.RefreshToken.ShouldNotBeNull();
        reloginResult.Token.ShouldNotBeNull();
        reloginResult.IsFounder.ShouldBeFalse();
        reloginResult.PrivacyPolicyAccepted.ShouldBeTrue();
        reloginResult.TermsAccepted.ShouldBeTrue();
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnNotFound_WhenUrlIsInvalid()
    {
        var anonClient = _factory.CreateClient();
        var request = new LoginRequest("testEmail@email.com", "testPassword");
        var loginResponse = await anonClient.PostAsJsonAsync("/account/login", request);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();
        loginResult.ShouldNotBeNull();
        var authClient = _factory.CreateAuthenticatedClient(loginResult.Id);
        var resetPasswordRequest = new ResetPasswordRequest("newPassword");

        var response = await authClient.PutAsJsonAsync("/invalid url", resetPasswordRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCurrentAccount_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var response = await client.GetAsync("/account/current");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentAccount_ShouldReturnLoggedInUser_WhenAuthenticated()
    {
        //Arrange
        var anonClient = _factory.CreateClient();
        var request = new LoginRequest("testEmail@email.com", "testPassword");
        var loginResponse = await anonClient.PostAsJsonAsync("/account/login", request);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();
        loginResult.ShouldNotBeNull();
        var authClient = _factory.CreateAuthenticatedClient(loginResult.Id);

        //Act
        var response = await authClient.GetAsync("/account/current");

        //Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var currentAccount = await response.Content.ReadFromJsonAsync<AccountDto>();
        currentAccount.ShouldNotBeNull();
        currentAccount.Id.ShouldNotBe(Guid.Empty);
        currentAccount.Email.ShouldNotBeNull();
        currentAccount.Username.ShouldNotBeNull();
        currentAccount.RefreshToken.ShouldBeNull();
        currentAccount.Token.ShouldBeNull();
        currentAccount.IsFounder.ShouldBeFalse();
        currentAccount.PrivacyPolicyAccepted.ShouldBeTrue();
        currentAccount.TermsAccepted.ShouldBeTrue();
    }

    [Fact]
    public async Task GetCurrentAccount_ShouldReturnNotFound_WhenUrlIsInvalid()
    {
        //Arrange
        var anonClient = _factory.CreateClient();
        var request = new LoginRequest("testEmail@email.com", "testPassword");
        var loginResponse = await anonClient.PostAsJsonAsync("/account/login", request);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();
        loginResult.ShouldNotBeNull();
        var authClient = _factory.CreateAuthenticatedClient(loginResult.Id);

        //Act
        var response = await authClient.GetAsync("/invalid url");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        //Arrange
        var client = _factory.CreateClient();

        //Act
        var response = await client.DeleteAsync("/account");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnNoContent_WhenAuthenticated()
    {
        //Arrange
        var anonClient = _factory.CreateClient();
        var request = new LoginRequest("testEmail@email.com", "testPassword");
        var loginResponse = await anonClient.PostAsJsonAsync("/account/login", request);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();
        loginResult.ShouldNotBeNull();
        var authClient = _factory.CreateAuthenticatedClient(loginResult.Id);

        //Act
        var response = await authClient.DeleteAsync("/account");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnNotFound_WhenRelogin()
    {
        //Arrange
        var anonClient = _factory.CreateClient();
        var request = new LoginRequest("testEmail@email.com", "testPassword");
        var loginResponse = await anonClient.PostAsJsonAsync("/account/login", request);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();
        loginResult.ShouldNotBeNull();
        var authClient = _factory.CreateAuthenticatedClient(loginResult.Id);

        //Act
        var response = await authClient.DeleteAsync("/account");
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var reloginResponse = await anonClient.PostAsJsonAsync("/account/login", request);

        //Assert
        reloginResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnNotFound_WhenUrlIsInvalid()
    {
        //Arrange
        var anonClient = _factory.CreateClient();
        var request = new LoginRequest("testEmail@email.com", "testPassword");
        var loginResponse = await anonClient.PostAsJsonAsync("/account/login", request);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AccountDto>();
        loginResult.ShouldNotBeNull();
        var authClient = _factory.CreateAuthenticatedClient(loginResult.Id);

        //Act
        var response = await authClient.DeleteAsync("/invalid url");

        //Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}