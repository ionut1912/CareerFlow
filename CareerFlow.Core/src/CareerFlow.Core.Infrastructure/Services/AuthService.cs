using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Gateways.Dtos;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Modles;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareerFlow.Core.Infrastructure.Services;

public partial class AuthService : IAuthService
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
        GoogleUserDto googleUser = await _googleValidator.ValidateIdTokenAsync(idToken, _settings.Google.ClientId);
        Account? account = await _accountRepository.GetAccountByEmailAsync(googleUser.Email, cancellationToken);

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

        HttpResponseMessage tokenResponse = await _httpClient.PostAsync("https://www.linkedin.com/oauth/v2/accessToken",
            tokenReq, cancellationToken);
        tokenResponse.EnsureSuccessStatusCode();

        LiToken? tokenData = await tokenResponse.Content.ReadFromJsonAsync<LiToken>(cancellationToken);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenData!.AccessToken);

        LiUser? userData =
            await _httpClient.GetFromJsonAsync<LiUser>("https://api.linkedin.com/v2/userinfo", cancellationToken);
        Account? account = await _accountRepository.GetAccountByEmailAsync(userData!.Email, cancellationToken);

        if (account == null) return await SaveUserAsync(userData.Email, userData.Name, cancellationToken);

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

        HttpResponseMessage response =
            await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenReq, cancellationToken);
        response.EnsureSuccessStatusCode();

        JsonElement data = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        return data.GetProperty("id_token").GetString()!;
    }

    private async Task<Account> SaveUserAsync(string email, string name, CancellationToken cancellationToken)
    {
        LogNoAccountFound(email);
        Account account = CreateAccount(email, name);
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

    [LoggerMessage(Level = LogLevel.Information, Message = "No account found for email {Email}")]
    private partial void LogNoAccountFound(string email);
}
