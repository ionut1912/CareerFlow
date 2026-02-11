using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models;
using CareerFlow.Core.Infrastructure.Configurations;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace CareerFlow.Core.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IAccountRepository _accountRepositroy;
    private readonly HttpClient _httpClient;
    private readonly SocialAuthSettings _settings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IAccountRepository accountRepository, HttpClient httpClient, IOptions<SocialAuthSettings> settings, ILogger<AuthService> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));

        ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
        ArgumentNullException.ThrowIfNull(settings, nameof(settings));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _accountRepositroy = accountRepository;
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Account> LoginWithGoogleAsync(string idToken, CancellationToken ct = default)
    {
        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new List<string> { _settings.Google.ClientId }
        };
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
        var account = await _accountRepositroy.GetAccountByEmailAsync(payload.Email, ct);

        if (account == null)
        {
            _logger.LogInformation("No account found for email {Email}", payload.Email);
            account = CreateAccount(payload.Email, payload.GivenName);
            await _accountRepositroy.AddAsync(account, ct);
        }
        return account;
    }

    public async Task<Account> LoginWithLinkedInAsync(string authorizationCode, CancellationToken ct = default)
    {
        var tokenReq = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", authorizationCode },
            { "redirect_uri", _settings.LinkedIn.RedirectUri },
            { "client_id", _settings.LinkedIn.ClientId },
            { "client_secret", _settings.LinkedIn.ClientSecret }
        });
        var tokenRsponse = await _httpClient.PostAsync("https://www.linkedin.com/oauth/v2/accessToken", tokenReq, ct);
        tokenRsponse.EnsureSuccessStatusCode();
        var tokenData = await tokenRsponse.Content.ReadFromJsonAsync<LIToken>(cancellationToken: ct);
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData!.access_token);
        var userData = await _httpClient.GetFromJsonAsync<LIUser>("https://api.linkedin.com/v2/userinfo", ct);
        var account = await _accountRepositroy.GetAccountByEmailAsync(userData!.email, ct);
        if (account == null)
        {

            _logger.LogInformation("No account found for email {Email}", userData.email);
            account = CreateAccount(userData.email, userData.name);
            await _accountRepositroy.AddAsync(account, ct);
        }

        return account;
    }

    private static Account CreateAccount(string email, string? name)
    {
        var account = Account.Create(email, "SocialAccountPassword", name ?? "SocialLoginUser");
        account.AcceptTerms();
        account.AcceptPrivacyPolicy();
        return account;
    }
}
