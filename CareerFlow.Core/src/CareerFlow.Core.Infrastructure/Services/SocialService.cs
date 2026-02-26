using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Infrastructure.Services;

public class SocialService(
    IOptions<SocialAuthSettings> options,
    IAuthService authService,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork) : ISocialService
{
    private const string MobileAppDeepLinkFormat = "careerflowui://auth/callback?token={0}&refreshToken={1}";
    private readonly SocialAuthSettings _settings = options.Value;

    public string GoogleMobileLogin()
    {
        var redirectUri = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/google/mobile/callback");
        var scope = Uri.EscapeDataString("openid email profile");

        return
            $"https://accounts.google.com/o/oauth2/v2/auth?client_id={_settings.Google.ClientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}&access_type=offline";
    }

    public async Task<string> GoogleMobileCallBack(string code, CancellationToken cancellationToken)
    {
        var idToken = await authService.ExchangeGoogleCodeAsync(code, cancellationToken);
        var account = await authService.LoginWithGoogleAsync(idToken, cancellationToken);

        return await ProcessAccountAndGenerateCallbackUriAsync(account, cancellationToken);
    }

    public string LinkedInMobileLogin()
    {
        var redirectUri = Uri.EscapeDataString($"{_settings.BaseUrl}/social/auth/linkedin/mobile/callback");
        var scope = Uri.EscapeDataString("openid profile email");

        return
            $"https://www.linkedin.com/oauth/v2/authorization?client_id={_settings.LinkedIn.ClientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}";
    }

    public async Task<string> LinkedInCallBack(string code, CancellationToken cancellationToken)
    {
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
}