using System.Security.Cryptography;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CareerFlow.Core.Infrastructure.Services;

public class SocialService(
    IOptions<SocialAuthSettings> options,
    IAuthService authService,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IMemoryCache cache) : ISocialService
{
    private readonly SocialAuthSettings _settings = options.Value;

    // 1. Am adăugat parametrul returnUrl
    public string GoogleMobileLogin(string? returnUrl = null)
    {
        var redirectUri = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/google/mobile/callback");
        var scope = Uri.EscapeDataString("openid email profile");

        // 2. Pasăm adresa către funcția care o stochează
        var state = GenerateAndStoreState(returnUrl);

        return
            $"https://accounts.google.com/o/oauth2/v2/auth?client_id={_settings.Google.ClientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}&access_type=offline&state={state}";
    }

    public async Task<string> GoogleMobileCallBackAsync(string code, string state, CancellationToken cancellationToken)
    {
        // 3. Recuperăm adresa de la Expo Go din memorie
        var savedReturnUrl = ValidateStateAndGetReturnUrl(state);

        var idToken = await authService.ExchangeGoogleCodeAsync(code, cancellationToken);
        var account = await authService.LoginWithGoogleAsync(idToken, cancellationToken);

        // 4. Folosim adresa recuperată pentru a genera link-ul final
        return await ProcessAccountAndGenerateCallbackUriAsync(account, savedReturnUrl, cancellationToken);
    }

    // --- Identic pentru LinkedIn ---
    public string LinkedInMobileLogin(string? returnUrl = null)
    {
        var redirectUri = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/linkedin/mobile/callback");
        var scope = Uri.EscapeDataString("openid profile email");
        var state = GenerateAndStoreState(returnUrl);

        return
            $"https://www.linkedin.com/oauth/v2/authorization?client_id={_settings.LinkedIn.ClientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}&state={state}";
    }

    public async Task<string> LinkedInCallBackAsync(string code, string state, CancellationToken cancellationToken)
    {
        var savedReturnUrl = ValidateStateAndGetReturnUrl(state);
        var account = await authService.LoginWithLinkedInAsync(code, cancellationToken);
        return await ProcessAccountAndGenerateCallbackUriAsync(account, savedReturnUrl, cancellationToken);
    }

    // --- Metodele ajutătoare modificate ---

    private async Task<string> ProcessAccountAndGenerateCallbackUriAsync(Account account, string returnUrl,
        CancellationToken cancellationToken)
    {
        var jwt = tokenService.GenerateToken(account);
        var refreshToken = tokenService.GenerateRefreshToken(account.Id, jwt.Token);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Lipim dinamic tokenii la adresa primită
        var separator = returnUrl.Contains("?") ? "&" : "?";
        return $"{returnUrl}{separator}token={jwt.Token}&refreshToken={refreshToken.TokenHash}";
    }

    private string GenerateAndStoreState(string? returnUrl)
    {
        var stateBytes = RandomNumberGenerator.GetBytes(32);
        var state = Convert.ToBase64String(stateBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        // Dacă nu primim link (ex: din producție), folosim o adresă implicită sigură
        var urlToSave = string.IsNullOrWhiteSpace(returnUrl) ? "careerflow://auth/callback" : returnUrl;

        // Salvăm URL-ul în cache în loc de un simplu "true"
        cache.Set(state, urlToSave, TimeSpan.FromMinutes(15));

        return state;
    }

    private string ValidateStateAndGetReturnUrl(string state)
    {
        // Verificăm state-ul și extragem direct adresa salvată
        if (string.IsNullOrWhiteSpace(state) || !cache.TryGetValue(state, out string? returnUrl) ||
            string.IsNullOrEmpty(returnUrl))
            throw new InvalidOperationException("Invalid or missing state parameter. CSRF validation failed.");

        cache.Remove(state);
        return returnUrl;
    }
}