using System.Net;
using System.Net.Http.Json;
using CareerFlow.Core.Api.IntegrationTests.Setup;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests;

[Trait("Category", "Integration")]
public class AccountEndpointsTests : IntegrationTestBase
{
    public AccountEndpointsTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_ShouldReturnSuccess_WhenDataIsValid()
    {
        var request = new CreateAccountRequest("newEmail@email.com", "testPassword", "newUsername", "testName");

        var response = await AnonymousClient.PostAsJsonAsync("/account/register", request);

        var result = await response.Content.ReadFromJsonAsync<Guid>();
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenDataIsInvalid()
    {
        var request = new CreateAccountRequest("testEmail", "", "testUsername", "testName");

        var response = await AnonymousClient.PostAsJsonAsync("/account/register", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnNotFound_WhenUrlIsInvalid()
    {
        var request = new CreateAccountRequest("testEmail", "testPassword", "testUsername", "testName");

        var response = await AnonymousClient.PostAsJsonAsync("/invalid-ur;", request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenAccountExists()
    {
        var request = new CreateAccountRequest("testEmail@email.com", "testPassword", "testUsername", "testName");
        await AnonymousClient.PostAsJsonAsync("/account/register", request);

        var response = await AnonymousClient.PostAsJsonAsync("/account/register", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        var request = new LoginRequest("testEmail", "testPassword");

        var response = await AnonymousClient.PostAsJsonAsync("/account/login", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Return200_WhenDataIsValid()
    {
        var (_, _, credentials) = await CreateAndAuthenticateUserAsync();

        var response = await AnonymousClient.PostAsJsonAsync("/account/login", credentials);

        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AccountDto>();
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Email.ShouldBe(credentials.Email);
        result.Username.ShouldNotBe(null);
        result.RefreshToken.ShouldNotBe(null);
        result.IsFounder.ShouldBeFalse();
        result.PrivacyPolicyAccepted.ShouldBeTrue();
        result.TermsAccepted.ShouldBeTrue();
    }

    [Fact]
    public async Task Login_ShouldReturnNotFound_WhenUrlIsInvalid()
    {
        var request = new LoginRequest("testEmail", "testPassword");

        var response = await AnonymousClient.PostAsJsonAsync("/invalid-uri", request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsInvalid()
    {
        await CreateAndAuthenticateUserAsync("testEmail@email.com", "testPassword");
        var request = new LoginRequest("testEmail@email.com", "testPassword2");

        var response = await AnonymousClient.PostAsJsonAsync("/account/login", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnNotFound_WhenEmailDoesNotExist()
    {
        var request = new LoginRequest("testEmail2@email.com", "testPassword2");

        var response = await AnonymousClient.PostAsJsonAsync("/account/login", request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenIsNotAuthenticated()
    {
        var request = new RefreshTokenRequest("testRefresh", "testRefreshToken");

        var response = await AnonymousClient.PostAsJsonAsync("/account/refresh-token", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnOk_WhenDataIsValid()
    {
        var (authClient, account, _) = await CreateAndAuthenticateUserAsync();
        var refreshTokenRequest = new RefreshTokenRequest(account.Token, account.RefreshToken);

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
        var (authClient, account, _) = await CreateAndAuthenticateUserAsync();
        var refreshTokenRequest = new RefreshTokenRequest(account.Token, account.RefreshToken);

        var response = await authClient.PostAsJsonAsync("/invalidurl", refreshTokenRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnUnauthorized_WhenIsNotAuthenticated()
    {
        var request = new ResetPasswordRequest("newPassword");

        var response = await AnonymousClient.PutAsJsonAsync("/account/reset-password", request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenLoginWithOldPassword()
    {
        var (authClient, _, credentials) = await CreateAndAuthenticateUserAsync();
        var resetPasswordRequest = new ResetPasswordRequest("newPassword");

        var response = await authClient.PutAsJsonAsync("/account/reset-password", resetPasswordRequest);
        var reloginRequest = await AnonymousClient.PostAsJsonAsync("/account/login", credentials);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        reloginRequest.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnSuccess_WhenLoginWithNewPassword()
    {
        var (authClient, _, credentials) = await CreateAndAuthenticateUserAsync();
        var resetPasswordRequest = new ResetPasswordRequest("newPassword");

        var response = await authClient.PutAsJsonAsync("/account/reset-password", resetPasswordRequest);
        var newLoginRequest = new LoginRequest(credentials.Email, resetPasswordRequest.NewPassword);
        var reloginRequest = await AnonymousClient.PostAsJsonAsync("/account/login", newLoginRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
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
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var resetPasswordRequest = new ResetPasswordRequest("newPassword");

        var response = await authClient.PutAsJsonAsync("/invalid url", resetPasswordRequest);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCurrentAccount_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var response = await AnonymousClient.GetAsync("/account/current");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentAccount_ShouldReturnLoggedInUser_WhenAuthenticated()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();

        var response = await authClient.GetAsync("/account/current");

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
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();

        var response = await authClient.GetAsync("/invalid url");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var response = await AnonymousClient.DeleteAsync("/account");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnNoContent_WhenAuthenticated()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();

        var response = await authClient.DeleteAsync("/account");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnNotFound_WhenRelogin()
    {
        var (authClient, _, credentials) = await CreateAndAuthenticateUserAsync();

        var response = await authClient.DeleteAsync("/account");
        var reloginResponse = await AnonymousClient.PostAsJsonAsync("/account/login", credentials);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        reloginResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnNotFound_WhenUrlIsInvalid()
    {
        var (authClient, _, _) = await CreateAndAuthenticateUserAsync();

        var response = await authClient.DeleteAsync("/invalid url");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}