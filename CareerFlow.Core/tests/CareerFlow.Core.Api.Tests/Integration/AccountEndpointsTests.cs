using System.Net;
using System.Net.Http.Json;
using System.Reflection;

using CareerFlow.Core.Api.Tests.Setup;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests.Account;
using CareerFlow.Core.Application.Requests.LegalDoc;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace CareerFlow.Core.Api.Tests.Integration;

[Trait("Category", "Integration")]
public class AccountEndpointsTests(TestWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Register_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        var request = new CreateAccountRequest("newEmail@email.com", "testPassword", "testPassword", "newUsername",
            "testName");

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/account/register", request);
        Guid result = await response.Content.ReadFromJsonAsync<Guid>();

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPasswordsMisMATCH()
    {
        // Arrange
        var request = new CreateAccountRequest("newEmail@email.com", "testPassword", "testPassword2", "newUsername",
            "testName");

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/account/register", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenDataIsInvalid()
    {
        // Arrange
        var request = new CreateAccountRequest("testEmail", "", "", "testUsername", "testName");

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/account/register", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturnNotFound_WhenUrlIsInvalid()
    {
        // Arrange
        var request = new CreateAccountRequest("testEmail", "testPassword", "testPassword", "testUsername", "testName");

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/invalid-ur;", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenAccountExists()
    {
        // Arrange
        var request = new CreateAccountRequest("testEmail@email.com", "testPassword", "testPassword", "testUsername",
            "testName");
        await AnonymousClient.PostAsJsonAsync("/account/register", request);

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/account/register", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        // Arrange
        var request = new LoginRequest("testEmail", "testPassword");

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/account/login", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Return200_WhenDataIsValid()
    {
        // Arrange
        (_, _, LoginRequest credentials) = await CreateAndAuthenticateUserAsync();

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/account/login", credentials);
        AccountDto? result = await response.Content.ReadFromJsonAsync<AccountDto>();

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
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
        // Arrange
        var request = new LoginRequest("testEmail", "testPassword");

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/invalid-uri", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_ShouldReturnBadRequest_WhenPasswordIsInvalid()
    {
        // Arrange
        await CreateAndAuthenticateUserAsync("testEmail@email.com");
        var request = new LoginRequest("testEmail@email.com", "testPassword2");

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/account/login", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnNotFound_WhenEmailDoesNotExist()
    {
        // Arrange
        var request = new LoginRequest("testEmail2@email.com", "testPassword2");

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/account/login", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnUnauthorized_WhenIsNotAuthenticated()
    {
        // Arrange
        var request = new RefreshTokenRequest("testRefresh", "testRefreshToken");

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/account/refresh-token", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnOk_WhenDataIsValid()
    {
        // Arrange
        (HttpClient authClient, AccountDto account, _) = await CreateAndAuthenticateUserAsync();
        account.Token.ShouldNotBeNullOrWhiteSpace();
        account.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        var refreshTokenRequest = new RefreshTokenRequest(account.Token, account.RefreshToken);

        // Act
        HttpResponseMessage response = await authClient.PostAsJsonAsync("/account/refresh-token", refreshTokenRequest);
        RefreshTokenDto? result = await response.Content.ReadFromJsonAsync<RefreshTokenDto>();

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        result.ShouldNotBeNull();
        result.RefreshToken.ShouldNotBeNull();
        result.Token.ShouldNotBeNull();
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnNotFound_WhenUrlIsInvalid()
    {
        // Arrange
        (HttpClient authClient, AccountDto account, _) = await CreateAndAuthenticateUserAsync();
        account.Token.ShouldNotBeNullOrWhiteSpace();
        account.RefreshToken.ShouldNotBeNullOrWhiteSpace();
        var refreshTokenRequest = new RefreshTokenRequest(account.Token, account.RefreshToken);

        // Act
        HttpResponseMessage response = await authClient.PostAsJsonAsync("/invalidurl", refreshTokenRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ForgotPassword_ShouldReturnNoContent_WhenEmailExists()
    {
        // Arrange
        await CreateAndAuthenticateUserAsync("existing@email.com");
        var request = new ForgotPasswordRequest("existing@email.com");

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/account/forgot-password", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ForgotPassword_ShouldThrowNotFound_WhenEmailDoesNotExist()
    {
        // Arrange
        var request = new ForgotPasswordRequest("nonexistent@email.com");

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/account/forgot-password", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnNoContent_WhenTokenAndDataAreValid()
    {
        // Arrange
        const string email = "reset-success@email.com";
        await CreateAndAuthenticateUserAsync(email);
        const string rawToken = "SuperSecretToken123";

        using (IServiceScope scope = Factory.Services.CreateScope())
        {
            IAccountRepository repo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            IPasswordService passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
            IUnitOfWork uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            Account? account = await repo.GetAccountByEmailAsync(email, CancellationToken.None);
            account.ShouldNotBeNull();
            account.GenerateResetPasswordToken(rawToken, passwordService);
            repo.Update(account);
            await uow.SaveChangesAsync();
        }

        var resetRequest = new ResetPasswordRequest(email, "NewSecurePassword123!", rawToken);

        // Act
        HttpResponseMessage response = await AnonymousClient.PutAsJsonAsync("/account/reset-password", resetRequest);
        var loginRequest = new LoginRequest(email, "NewSecurePassword123!");
        HttpResponseMessage loginResponse = await AnonymousClient.PostAsJsonAsync("/account/login", loginRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        loginResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_ShouldReturnBadRequest_WhenTokenIsExpired()
    {
        // Arrange
        const string email = "expired@email.com";
        await CreateAndAuthenticateUserAsync(email);
        const string rawToken = "token123";

        using (IServiceScope scope = Factory.Services.CreateScope())
        {
            IAccountRepository repo = scope.ServiceProvider.GetRequiredService<IAccountRepository>();
            IPasswordService passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
            IUnitOfWork uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            Account? account = await repo.GetAccountByEmailAsync(email, CancellationToken.None);
            account.ShouldNotBeNull();
            account.GenerateResetPasswordToken(rawToken, passwordService);

            PropertyInfo? field = account.GetType().GetProperty("ResetPasswordTokenExpiresAt");
            field?.SetValue(account, DateTime.UtcNow.AddHours(-1));

            repo.Update(account);
            await uow.SaveChangesAsync();
        }

        var resetRequest = new ResetPasswordRequest(email, "NewPassword123!", rawToken);

        // Act
        HttpResponseMessage response = await AnonymousClient.PutAsJsonAsync("/account/reset-password", resetRequest);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCurrentAccount_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange & Act
        HttpResponseMessage response = await AnonymousClient.GetAsync("/account/current");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentAccount_ShouldReturnLoggedInUser_WhenAuthenticated()
    {
        // Arrange
        (HttpClient authClient, _, _) = await CreateAndAuthenticateUserAsync();

        // Act
        HttpResponseMessage response = await authClient.GetAsync("/account/current");
        AccountDto? currentAccount = await response.Content.ReadFromJsonAsync<AccountDto>();

        // Assert
        response.EnsureSuccessStatusCode();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        currentAccount.ShouldNotBeNull();
        currentAccount.Id.ShouldNotBe(Guid.Empty);
        currentAccount.Email.ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        // Arrange & Act
        HttpResponseMessage response = await AnonymousClient.DeleteAsync("/account");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnNoContent_WhenAuthenticated()
    {
        // Arrange
        (HttpClient authClient, _, _) = await CreateAndAuthenticateUserAsync();

        // Act
        HttpResponseMessage response = await authClient.DeleteAsync("/account");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteAccount_ShouldReturnNotFound_WhenRelogin()
    {
        // Arrange
        (HttpClient authClient, _, LoginRequest credentials) = await CreateAndAuthenticateUserAsync();

        // Act
        HttpResponseMessage response = await authClient.DeleteAsync("/account");
        HttpResponseMessage reloginResponse = await AnonymousClient.PostAsJsonAsync("/account/login", credentials);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        reloginResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    #region Accept Legal Tests

    [Fact]
    public async Task AcceptLegal_ShouldReturnNoContent_WhenTermsAreAccepted()
    {
        // Arrange
        (HttpClient authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var request = new AcceptLegalDocRequest("Terms");

        // Act
        HttpResponseMessage response = await authClient.PostAsJsonAsync("/account/accept-legaldoc", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AcceptLegal_ShouldReturnNoContent_WhenPrivacyIsAccepted()
    {
        // Arrange
        (HttpClient authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var request = new AcceptLegalDocRequest("Privacy");

        // Act
        HttpResponseMessage response = await authClient.PostAsJsonAsync("/account/accept-legaldoc", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AcceptLegal_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var request = new AcceptLegalDocRequest("Terms");

        // Act
        HttpResponseMessage response = await AnonymousClient.PostAsJsonAsync("/account/accept-legaldoc", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AcceptLegal_ShouldReturnNotFound_WhenAccountDoesNotExistInDatabase()
    {
        // Arrange
        var account = Account.Create("testEmail@email.com", "testPassword", "testUsername", "testName");
        HttpClient orphanedClient = CreateClientForNonExistentUser(account);
        var request = new AcceptLegalDocRequest("Terms");

        // Act
        HttpResponseMessage response = await orphanedClient.PostAsJsonAsync("/account/accept-legaldoc", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AcceptLegal_ShouldReturnBadRequest_WhenTypeIsInvalid()
    {
        // Arrange
        (HttpClient authClient, _, _) = await CreateAndAuthenticateUserAsync();
        var request = new AcceptLegalDocRequest("InvalidType");

        // Act
        HttpResponseMessage response = await authClient.PostAsJsonAsync("/account/accept-legaldoc", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    #endregion
}
