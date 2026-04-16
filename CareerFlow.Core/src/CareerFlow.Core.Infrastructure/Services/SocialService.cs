using System.Security.Cryptography;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
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
        var redirectUri = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/google/mobile/callback");
        var scope = Uri.EscapeDataString("openid email profile");
        var state = await GenerateAndStoreStateAsync(returnUrl);

        return $"https://accounts.google.com/o/oauth2/v2/auth?client_id={_settings.Google.ClientId}" +
               $"&redirect_uri={redirectUri}&response_type=code&scope={scope}&access_type=offline&state={state}";
    }

    public async Task<string> GoogleMobileCallBackAsync(string code, string state, CancellationToken cancellationToken)
    {
        var savedReturnUrl = await ValidateStateAndGetReturnUrlAsync(state, cancellationToken);
        
        // Fix: If state is invalid or missing, redirect to frontend with an error instead of crashing
        if (string.IsNullOrEmpty(savedReturnUrl))
        {
            return "careerflow://auth/callback?error=session_expired";
        }

        var idToken = await authService.ExchangeGoogleCodeAsync(code, cancellationToken);
        var account = await authService.LoginWithGoogleAsync(idToken, cancellationToken);
        return await ProcessAccountAndGenerateCallbackUriAsync(account, savedReturnUrl, cancellationToken);
    }

    public async Task<string> LinkedInMobileLogin(string? returnUrl = null)
    {
        var redirectUri = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/linkedin/mobile/callback");
        var scope = Uri.EscapeDataString("openid profile email");
        var state = await GenerateAndStoreStateAsync(returnUrl);

        return $"https://www.linkedin.com/oauth/v2/authorization?client_id={_settings.LinkedIn.ClientId}" +
               $"&redirect_uri={redirectUri}&response_type=code&scope={scope}&state={state}";
    }

    public async Task<string> LinkedInCallBackAsync(string code, string state, CancellationToken cancellationToken)
    {
        var savedReturnUrl = await ValidateStateAndGetReturnUrlAsync(state, cancellationToken);
        
        // Fix: If state is invalid or missing, redirect to frontend with an error instead of crashing
        if (string.IsNullOrEmpty(savedReturnUrl))
        {
            return "careerflow://auth/callback?error=session_expired";
        }

        var account = await authService.LoginWithLinkedInAsync(code, cancellationToken);
        return await ProcessAccountAndGenerateCallbackUriAsync(account, savedReturnUrl, cancellationToken);
    }

    private async Task<string> ProcessAccountAndGenerateCallbackUriAsync(
        Account account, string returnUrl, CancellationToken cancellationToken)
    {
        var jwt = tokenService.GenerateToken(account);
        var refreshToken = tokenService.GenerateRefreshToken(account.Id, jwt.Token);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var separator = returnUrl.Contains('?') ? "&" : "?";
        return $"{returnUrl}{separator}token={jwt.Token}&refreshToken={refreshToken.TokenHash}";
    }

    private async Task<string> GenerateAndStoreStateAsync(string? returnUrl)
    {
        var stateBytes = RandomNumberGenerator.GetBytes(32);
        var state = Convert.ToBase64String(stateBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var urlToSave = string.IsNullOrWhiteSpace(returnUrl)
            ? "careerflow://auth/callback"
            : returnUrl;

        await cache.SetAsync($"oauth_state:{state}", urlToSave, TimeSpan.FromMinutes(15));

        return state;
    }

    private async Task<string> ValidateStateAndGetReturnUrlAsync(string state, CancellationToken cancellationToken)
    {
        // Fix: Stop throwing exceptions for missing state
        if (string.IsNullOrWhiteSpace(state))
            return string.Empty;

        var key = $"oauth_state:{state}";
        var returnUrl = await cache.GetAsync<string>(key, cancellationToken);

        // Fix: Return empty string instead of crashing if key doesn't exist in Redis
        if (string.IsNullOrEmpty(returnUrl))
            return string.Empty;

        await cache.RemoveAsync(key, cancellationToken);
        return returnUrl;
    }
}