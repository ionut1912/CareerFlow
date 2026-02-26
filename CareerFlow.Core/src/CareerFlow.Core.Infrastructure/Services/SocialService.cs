using System.Security.Cryptography;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Infrastructure.Services;

public class SocialService(
    IOptions<SocialAuthSettings> options,
    IAuthService authService,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IMemoryCache cache) : ISocialService
{
    private const string MobileAppDeepLinkFormat = "careerflowui://auth/callback?token={0}&refreshToken={1}";
    private readonly SocialAuthSettings _settings = options.Value;

    public string GoogleMobileLogin()
    {
        var redirectUri = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/google/mobile/callback");
        var scope = Uri.EscapeDataString("openid email profile");
        var state = GenerateAndStoreState();

        return
            $"https://accounts.google.com/o/oauth2/v2/auth?client_id={_settings.Google.ClientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}&access_type=offline&state={state}";
    }

    public async Task<string> GoogleMobileCallBack(string code, string state, CancellationToken cancellationToken)
    {
        ValidateState(state);

        var idToken = await authService.ExchangeGoogleCodeAsync(code, cancellationToken);
        var account = await authService.LoginWithGoogleAsync(idToken, cancellationToken);

        return await ProcessAccountAndGenerateCallbackUriAsync(account, cancellationToken);
    }

    public string LinkedInMobileLogin()
    {
        var redirectUri = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/linkedin/mobile/callback");
        var scope = Uri.EscapeDataString("openid profile email");
        var state = GenerateAndStoreState();

        return
            $"https://www.linkedin.com/oauth/v2/authorization?client_id={_settings.LinkedIn.ClientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}&state={state}";
    }

    public async Task<string> LinkedInCallBack(string code, string state, CancellationToken cancellationToken)
    {
        ValidateState(state);

        var account = await authService.LoginWithLinkedInAsync(code, cancellationToken);

        return await ProcessAccountAndGenerateCallbackUriAsync(account, cancellationToken);
    }

    private async Task<string> ProcessAccountAndGenerateCallbackUriAsync(Account account,
        CancellationToken cancellationToken)
    {
        var jwt = tokenService.GenerateToken(account);
        var refreshToken = tokenService.GenerateRefreshToken(account.Id, jwt.Token);

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return string.Format(MobileAppDeepLinkFormat, jwt.Token, refreshToken.Token);
    }

    private string GenerateAndStoreState()
    {
        var stateBytes = RandomNumberGenerator.GetBytes(32);
        
        var state = Convert.ToBase64String(stateBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        cache.Set(state, true, TimeSpan.FromMinutes(15));

        return state;
    }

    private void ValidateState(string state)
    {
        if (string.IsNullOrWhiteSpace(state) || !cache.TryGetValue(state, out _))
        {
            throw new InvalidOperationException("Invalid or missing state parameter. CSRF validation failed.");
        }

        cache.Remove(state);
    }
}