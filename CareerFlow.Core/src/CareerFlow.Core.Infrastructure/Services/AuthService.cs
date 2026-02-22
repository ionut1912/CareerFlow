using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareerFlow.Core.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IGoogleTokenValidator _googleValidator;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthService> _logger;
    private readonly SocialAuthSettings _settings;

    public AuthService(
        IAccountRepository accountRepository,
        IGoogleTokenValidator googleValidator,
        HttpClient httpClient,
        IOptions<SocialAuthSettings> settings,
        ILogger<AuthService> logger)
    {
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(googleValidator);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(logger);

        _accountRepository = accountRepository;
        _googleValidator = googleValidator;
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<Account> LoginWithGoogleAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var googleUser = await _googleValidator.ValidateIdTokenAsync(idToken, _settings.Google.ClientId);
        var account = await _accountRepository.GetAccountByEmailAsync(googleUser.Email, cancellationToken);

        if (account == null) return await SaveUserAsync(googleUser.Email, googleUser.Name, cancellationToken);
        return account;
    }

    public async Task<Account> LoginWithLinkedInAsync(string authorizationCode,
        CancellationToken cancellationToken = default)
    {
        var tokenReq = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", authorizationCode },
            { "redirect_uri", _settings.LinkedIn.RedirectUri },
            { "client_id", _settings.LinkedIn.ClientId },
            { "client_secret", _settings.LinkedIn.ClientSecret }
        });

        var tokenResponse = await _httpClient.PostAsync("https://www.linkedin.com/oauth/v2/accessToken", tokenReq,
            cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();

        var tokenData = await tokenResponse.Content.ReadFromJsonAsync<LIToken>(cancellationToken);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenData!.access_token);

        var userData =
            await _httpClient.GetFromJsonAsync<LIUser>("https://api.linkedin.com/v2/userinfo", cancellationToken);
        var account = await _accountRepository.GetAccountByEmailAsync(userData!.email, cancellationToken);

        if (account == null) return await SaveUserAsync(userData.email, userData.name, cancellationToken);

        return account;
    }
    
    public async Task<string> ExchangeGoogleCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var tokenReq = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "code", code },
            { "client_id", _settings.Google.ClientId },
            { "client_secret", _settings.Google.ClientSecret },
            { "redirect_uri", $"{_settings.BaseUrl}/social/auth/google/mobile/callback" },
            { "grant_type", "authorization_code" }
        });

        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenReq, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        return data.GetProperty("id_token").GetString()!;
    }

    private async Task<Account> SaveUserAsync(string email, string name, CancellationToken cancellationToken)
    {
        _logger.LogInformation("No account found for email {Email}", email);
        var account = CreateAccount(email, name);
        await _accountRepository.AddAsync(account, cancellationToken);
        return account;
    }

    private static Account CreateAccount(string email, string? name)
    {
        var account = Account.Create(email, "SocialAccountPassword", "SocialLoginUser", name ?? "SocialUser");
        account.AcceptTerms();
        account.AcceptPrivacyPolicy();
        return account;
    }
}