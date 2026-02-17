using CareerFlow.Core.Api.IntegrationTests.Infrastructure;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using Shouldly;
using System.Net.Http.Json;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests;

[Collection("Account")]
public sealed class AccountEndpointGroupIntegrationTests : IntegrationTestBase
{
    private const string RegisterUrl = "/account/register";
    private const string LoginUrl = "/account/login";
    private const string GoogleUrl = "/account/google";
    private const string LinkedInUrl = "/account/linkedin";
    private const string RefreshUrl = "/account/refresh-token";
    private const string ResetPassUrl = "/account/reset-password";
    private const string CurrentUrl = "/account/current";
    private const string DeleteUrl = "/account";

    public AccountEndpointGroupIntegrationTests(SharedContainerFixture containers)
        : base(containers) { }

    [Fact]
    public async Task Register_ValidRequest_Returns200OkWithNonEmptyGuid()
    {
        var client = CreateClient();
        var request = new CreateAccountRequest("register-happy@example.com", "V4lid!Pass1", "Happy User", "name");

        var response = await client.PostAsJsonAsync(RegisterUrl, request);

        var id = await ShouldBeOkWithAsync<Guid>(response);
        id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns4xx()
    {
        var client = CreateClient();
        var request = new CreateAccountRequest("duplicate@example.com", "V4lid!Pass1", "username", "name");

        var first = await client.PostAsJsonAsync(RegisterUrl, request);
        first.IsSuccessStatusCode.ShouldBeTrue();
        var second = await client.PostAsJsonAsync(RegisterUrl, request);

        ((int)second.StatusCode).ShouldBeInRange(400, 422);
    }

    [Fact]
    public async Task Register_InvalidEmailFormat_Returns400BadRequest()
    {
        var client = CreateClient();
        var request = new CreateAccountRequest("not-an-email", "V4lid!Pass1", "User", "noname");

        var response = await client.PostAsJsonAsync(RegisterUrl, request);

        ShouldBeBadRequest(response);
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400BadRequest()
    {
        var client = CreateClient();
        var request = new CreateAccountRequest("weak@example.com", "password", "User", "name");

        var response = await client.PostAsJsonAsync(RegisterUrl, request);

        ShouldBeBadRequest(response);
    }

    [Fact]
    public async Task Register_MissingBody_Returns400BadRequest()
    {
        var client = CreateClient();

        var response = await client.PostAsync(RegisterUrl, content: null);

        ShouldBeBadRequest(response);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200OkWithAccountDtoAndTokens()
    {
        var client = CreateClient();
        await SeedAccountAsync(client, "login-happy@example.com", "Login User", "V4lid!Pass1");

        var response = await client.PostAsJsonAsync(LoginUrl,
            new LoginRequest("login-happy@example.com", "V4lid!Pass1"));

        var dto = await ShouldBeOkWithAsync<AccountDto>(response);
        dto.Email.ShouldBe("login-happy@example.com");
        dto.Token.ShouldNotBeNullOrWhiteSpace();
        dto.RefreshToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401Unauthorized()
    {
        var client = CreateClient();
        await SeedAccountAsync(client, "wrong-pass@example.com", "User", "Correct!Pass1");

        var response = await client.PostAsJsonAsync(LoginUrl,
            new LoginRequest("wrong-pass@example.com", "Wrong!Pass99"));

        ShouldBeUnauthorized(response);
    }

    [Fact]
    public async Task Login_NonExistentEmail_Returns401Unauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(LoginUrl,
            new LoginRequest("ghost@example.com", "AnyP@ss1!"));

        ShouldBeUnauthorized(response);
    }

    [Fact]
    public async Task Login_MissingBody_Returns400BadRequest()
    {
        var client = CreateClient();

        var response = await client.PostAsync(LoginUrl, content: null);

        ShouldBeBadRequest(response);
    }

    [Fact]
    public async Task LoginWithGoogle_InvalidToken_Returns4xxNoServerError()
    {
        var client = CreateClient();
        var request = new GoogleLoginRequest("INVALID_GOOGLE_ID_TOKEN");

        var response = await client.PostAsJsonAsync(GoogleUrl, request);

        ((int)response.StatusCode).ShouldBeInRange(400, 499);
    }

    [Fact]
    public async Task LoginWithGoogle_MissingBody_Returns400BadRequest()
    {
        var client = CreateClient();

        var response = await client.PostAsync(GoogleUrl, content: null);

        ShouldBeBadRequest(response);
    }

    [Fact]
    public async Task LoginWithLinkedin_InvalidToken_Returns4xxNoServerError()
    {
        var client = CreateClient();
        var request = new LinkedInLoginRequest("INVALID_LINKEDIN_TOKEN");

        var response = await client.PostAsJsonAsync(LinkedInUrl, request);

        ((int)response.StatusCode).ShouldBeInRange(400, 499);
    }

    [Fact]
    public async Task LoginWithLinkedin_MissingBody_Returns400BadRequest()
    {
        var client = CreateClient();

        var response = await client.PostAsync(LinkedInUrl, content: null);

        ShouldBeBadRequest(response);
    }

    [Fact]
    public async Task RefreshToken_ValidToken_Returns200OkWithNewRotatedTokenPair()
    {
        var client = CreateClient();
        await SeedAccountAsync(client, "refresh-happy@example.com", "Refresh User", "V4lid!Pass1");
        var loginResp = await client.PostAsJsonAsync(LoginUrl,
            new LoginRequest("refresh-happy@example.com", "V4lid!Pass1"));
        var loginDto = await ShouldBeOkWithAsync<AccountDto>(loginResp);

        var response = await client.PostAsJsonAsync(RefreshUrl,
            new RefreshTokenRequest(loginDto.Token, loginDto.RefreshToken));

        var dto = await ShouldBeOkWithAsync<RefreshTokenDto>(response);
        dto.Token.ShouldNotBeNullOrWhiteSpace();
        dto.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        dto.RefreshToken.ShouldNotBe(loginDto.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_InvalidToken_Returns401Unauthorized()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(RefreshUrl,
            new RefreshTokenRequest("completely-invalid-token-xyz", "invalid-refreshtoken"));

        ShouldBeUnauthorized(response);
    }

    [Fact]
    public async Task RefreshToken_MissingBody_Returns400BadRequest()
    {
        var client = CreateClient();

        var response = await client.PostAsync(RefreshUrl, content: null);

        ShouldBeBadRequest(response);
    }

    [Fact]
    public async Task GetCurrentAccount_AuthenticatedUser_Returns200OkWithOwnAccountDto()
    {
        var anonClient = CreateClient();
        var accountId = await SeedAccountAndGetIdAsync(
            anonClient, "current-happy@example.com", "Current User", "V4lid!Pass1");
        var authedClient = CreateAuthenticatedClient(accountId);

        var response = await authedClient.GetAsync(CurrentUrl);

        var dto = await ShouldBeOkWithAsync<AccountDto>(response);
        dto.Id.ShouldBe(accountId);
        dto.Email.ShouldBe("current-happy@example.com");
    }

    [Fact]
    public async Task GetCurrentAccount_UnauthenticatedRequest_Returns401Unauthorized()
    {
        var client = CreateClient();

        var response = await client.GetAsync(CurrentUrl);

        ShouldBeUnauthorized(response);
    }

    [Fact]
    public async Task GetCurrentAccount_EmptyGuidInClaims_Returns401Unauthorized()
    {
        var client = CreateAuthenticatedClient(Guid.Empty);

        var response = await client.GetAsync(CurrentUrl);

        ShouldBeUnauthorized(response);
    }

    [Fact]
    public async Task ResetPassword_ValidRequest_Returns204NoContent()
    {
        var anonClient = CreateClient();
        var accountId = await SeedAccountAndGetIdAsync(
            anonClient, "reset-happy@example.com", "Reset User", "0ld!Pass1");
        var authedClient = CreateAuthenticatedClient(accountId);

        var response = await authedClient.PutAsJsonAsync(ResetPassUrl,
            new ResetPasswordRequest("N3w!Pass2"));

        ShouldBeNoContent(response);
    }

    [Fact]
    public async Task ResetPassword_AfterReset_OldPasswordIsRejected()
    {
        var anonClient = CreateClient();
        var accountId = await SeedAccountAndGetIdAsync(
            anonClient, "reset-old@example.com", "User", "0ld!Pass1");
        var authedClient = CreateAuthenticatedClient(accountId);
        await authedClient.PutAsJsonAsync(ResetPassUrl, new ResetPasswordRequest("N3w!Pass2"));

        var response = await anonClient.PostAsJsonAsync(LoginUrl,
            new LoginRequest("reset-old@example.com", "0ld!Pass1"));

        ShouldBeUnauthorized(response);
    }

    [Fact]
    public async Task ResetPassword_AfterReset_NewPasswordIsAccepted()
    {
        var anonClient = CreateClient();
        var accountId = await SeedAccountAndGetIdAsync(
            anonClient, "reset-new@example.com", "User", "0ld!Pass1");
        var authedClient = CreateAuthenticatedClient(accountId);
        await authedClient.PutAsJsonAsync(ResetPassUrl, new ResetPasswordRequest("N3w!Pass2"));

        var response = await anonClient.PostAsJsonAsync(LoginUrl,
            new LoginRequest("reset-new@example.com", "N3w!Pass2"));

        var dto = await ShouldBeOkWithAsync<AccountDto>(response);
        dto.Token.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ResetPassword_WrongOldPassword_Returns4xx()
    {
        var anonClient = CreateClient();
        var accountId = await SeedAccountAndGetIdAsync(
            anonClient, "reset-wrong@example.com", "User", "Correct!Pass1");
        var authedClient = CreateAuthenticatedClient(accountId);

        var response = await authedClient.PutAsJsonAsync(ResetPassUrl,
            new ResetPasswordRequest("N3w!Pass2"));

        ((int)response.StatusCode).ShouldBeInRange(400, 422);
    }

    [Fact]
    public async Task ResetPassword_UnauthenticatedRequest_Returns401Unauthorized()
    {
        var client = CreateClient();

        var response = await client.PutAsJsonAsync(ResetPassUrl,
            new ResetPasswordRequest("N3w!Pass2"));

        ShouldBeUnauthorized(response);
    }

    [Fact]
    public async Task ResetPassword_EmptyGuidInClaims_Returns401Unauthorized()
    {
        var client = CreateAuthenticatedClient(Guid.Empty);

        var response = await client.PutAsJsonAsync(ResetPassUrl,
            new ResetPasswordRequest("N3w!Pass2"));

        ShouldBeUnauthorized(response);
    }

    [Fact]
    public async Task ResetPassword_MissingBody_Returns400BadRequest()
    {
        var client = CreateAuthenticatedClient(Guid.NewGuid());

        var response = await client.PutAsync(ResetPassUrl, content: null);

        ShouldBeBadRequest(response);
    }

    [Fact]
    public async Task DeleteUserAccount_AuthenticatedUser_Returns204NoContent()
    {
        var anonClient = CreateClient();
        var accountId = await SeedAccountAndGetIdAsync(
            anonClient, "delete-happy@example.com", "Delete User", "V4lid!Pass1");
        var authedClient = CreateAuthenticatedClient(accountId);

        var response = await authedClient.DeleteAsync(DeleteUrl);

        ShouldBeNoContent(response);
    }

    [Fact]
    public async Task DeleteUserAccount_AfterDeletion_GetCurrentReturns4xx()
    {
        var anonClient = CreateClient();
        var accountId = await SeedAccountAndGetIdAsync(
            anonClient, "delete-verify@example.com", "User", "V4lid!Pass1");
        var authedClient = CreateAuthenticatedClient(accountId);
        var deleteResp = await authedClient.DeleteAsync(DeleteUrl);
        ShouldBeNoContent(deleteResp);

        var response = await authedClient.GetAsync(CurrentUrl);

        ((int)response.StatusCode).ShouldBeInRange(401, 404);
    }

    [Fact]
    public async Task DeleteUserAccount_AfterDeletion_LoginIsRejected()
    {
        var anonClient = CreateClient();
        var accountId = await SeedAccountAndGetIdAsync(
            anonClient, "delete-login@example.com", "User", "V4lid!Pass1");
        var authedClient = CreateAuthenticatedClient(accountId);
        await authedClient.DeleteAsync(DeleteUrl);

        var response = await anonClient.PostAsJsonAsync(LoginUrl,
            new LoginRequest("delete-login@example.com", "V4lid!Pass1"));

        ShouldBeUnauthorized(response);
    }

    [Fact]
    public async Task DeleteUserAccount_UnauthenticatedRequest_Returns401Unauthorized()
    {
        var client = CreateClient();

        var response = await client.DeleteAsync(DeleteUrl);

        ShouldBeUnauthorized(response);
    }

    [Fact]
    public async Task DeleteUserAccount_EmptyGuidInClaims_Returns401Unauthorized()
    {
        var client = CreateAuthenticatedClient(Guid.Empty);

        var response = await client.DeleteAsync(DeleteUrl);

        ShouldBeUnauthorized(response);
    }

    [Fact]
    public async Task Account_FullLifecycle_RegisterLoginCurrentResetDelete_IsConsistent()
    {
        var anonClient = CreateClient();
        const string email = "lifecycle@example.com";
        const string oldPass = "0ld!Pass1";
        const string newPass = "N3w!Pass2";

        var registerResp = await anonClient.PostAsJsonAsync(RegisterUrl,
            new CreateAccountRequest(email, oldPass, "Lifecycle User", "name"));
        var accountId = await ShouldBeOkWithAsync<Guid>(registerResp);
        accountId.ShouldNotBe(Guid.Empty);

        var loginResp = await anonClient.PostAsJsonAsync(LoginUrl,
            new LoginRequest(email, oldPass));
        var loginDto = await ShouldBeOkWithAsync<AccountDto>(loginResp);
        loginDto.Email.ShouldBe(email);
        loginDto.Token.ShouldNotBeNullOrWhiteSpace();

        var authedClient = CreateAuthenticatedClient(accountId);
        var currentResp = await authedClient.GetAsync(CurrentUrl);
        var currentDto = await ShouldBeOkWithAsync<AccountDto>(currentResp);
        currentDto.Id.ShouldBe(accountId);

        var resetResp = await authedClient.PutAsJsonAsync(ResetPassUrl,
            new ResetPasswordRequest(newPass));
        ShouldBeNoContent(resetResp);

        var newLoginResp = await anonClient.PostAsJsonAsync(LoginUrl,
            new LoginRequest(email, newPass));
        var newLoginDto = await ShouldBeOkWithAsync<AccountDto>(newLoginResp);
        newLoginDto.Token.ShouldNotBeNullOrWhiteSpace();

        var deleteResp = await authedClient.DeleteAsync(DeleteUrl);
        ShouldBeNoContent(deleteResp);
    }

    private async Task<Guid> SeedAccountAndGetIdAsync(
        HttpClient client, string email, string name, string password)
    {
        var response = await client.PostAsJsonAsync(RegisterUrl,
            new CreateAccountRequest(email, password, "username", name));
        response.IsSuccessStatusCode.ShouldBeTrue(
            $"Seed failed for '{email}': {(int)response.StatusCode}");
        var id = await response.Content.ReadFromJsonAsync<Guid>();
        id.ShouldNotBe(Guid.Empty);
        return id;
    }

    private async Task SeedAccountAsync(
        HttpClient client, string email, string name, string password)
        => await SeedAccountAndGetIdAsync(client, email, name, password);
}