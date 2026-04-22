using System.Security.Cryptography;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Constants;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.Authentication;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace CareerFlow.Core.Infrastructure.Services;

public class SocialService(
    IOptions<SocialAuthSettings> options,
    IAuthService authService,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ICacheService cache) : ISocialService
{
    private readonly SocialAuthSettings _settings = options.Value;

    public async Task<string> GoogleMobileLogin(string? returnUrl = null)
    {
        string redirectUri = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/google/mobile/callback");
        string scope = Uri.EscapeDataString("openid email profile");
        string state = await GenerateAndStoreStateAsync(returnUrl);

        return $"https://accounts.google.com/o/oauth2/v2/auth?client_id={_settings.Google.ClientId}" +
               $"&redirect_uri={redirectUri}&response_type=code&scope={scope}&access_type=offline&state={state}";
    }

    public async Task<string> GoogleMobileCallBackAsync(string code, string state, CancellationToken cancellationToken)
    {
        string savedReturnUrl = await ValidateStateAndGetReturnUrlAsync(state);

        if (string.IsNullOrEmpty(savedReturnUrl)) return "careerflow://auth/callback?error=session_expired";

        try
        {
            string idToken = await authService.ExchangeGoogleCodeAsync(code, cancellationToken);
            Account account = await authService.LoginWithGoogleAsync(idToken, cancellationToken);
            return await ProcessAccountAndGenerateCallbackUriAsync(account, savedReturnUrl, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            return "careerflow://auth/callback?error=duplicate_request";
        }
    }

    public async Task<string> LinkedInMobileLogin(string? returnUrl = null)
    {
        string redirectUri = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/linkedin/mobile/callback");
        string scope = Uri.EscapeDataString("openid profile email");
        string state = await GenerateAndStoreStateAsync(returnUrl);

        return $"https://www.linkedin.com/oauth/v2/authorization?client_id={_settings.LinkedIn.ClientId}" +
               $"&redirect_uri={redirectUri}&response_type=code&scope={scope}&state={state}";
    }

    public async Task<string> LinkedInCallBackAsync(string code, string state, CancellationToken cancellationToken)
    {
        string savedReturnUrl = await ValidateStateAndGetReturnUrlAsync(state);

        if (string.IsNullOrEmpty(savedReturnUrl)) return "careerflow://auth/callback?error=session_expired";

        try
        {
            Account account = await authService.LoginWithLinkedInAsync(code, cancellationToken);
            return await ProcessAccountAndGenerateCallbackUriAsync(account, savedReturnUrl, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            return "careerflow://auth/callback?error=duplicate_request";
        }
    }

    private async Task<string> ProcessAccountAndGenerateCallbackUriAsync(
        Account account, string returnUrl, CancellationToken cancellationToken)
    {
        AuthResult jwt = tokenService.GenerateToken(account);
        RefreshToken refreshToken = tokenService.GenerateRefreshToken(account.Id, jwt.Token);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        string separator = returnUrl.Contains('?') ? "&" : "?";
        return $"{returnUrl}{separator}token={jwt.Token}&refreshToken={refreshToken.TokenHash}";
    }

    private async Task<string> GenerateAndStoreStateAsync(string? returnUrl)
    {
        byte[] stateBytes = RandomNumberGenerator.GetBytes(32);
        string state = Convert.ToBase64String(stateBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        string urlToSave = string.IsNullOrWhiteSpace(returnUrl)
            ? "careerflow://auth/callback"
            : returnUrl;

        await cache.SetAsync(CacheKeyConstants.CacheKeyState(state), urlToSave, TimeSpan.FromMinutes(15));

        return state;
    }

    private async Task<string> ValidateStateAndGetReturnUrlAsync(string state)
    {
        if (string.IsNullOrWhiteSpace(state))
            return string.Empty;

        string key = CacheKeyConstants.CacheKeyState(state);
        string? returnUrl = await cache.GetAsync<string>(key);

        return string.IsNullOrEmpty(returnUrl) ? string.Empty : returnUrl;
    }
}
