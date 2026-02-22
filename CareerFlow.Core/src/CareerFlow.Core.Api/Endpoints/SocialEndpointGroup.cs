using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Shared.Api.Endpoints;
using Shared.Api.Infrastructure;

namespace CareerFlow.Core.Api.Endpoints;

public class SocialEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(this);
        group.MapGet(GoogleMobileLogin, "/auth/google/mobile");
        group.MapGet(GoogleMobileCallback, "/auth/google/mobile/callback");
        group.MapGet(LinkedInMobileLogin, "/auth/linkedin/mobile");
        group.MapGet(LinkedInMobileCallback, "/auth/linkedin/mobile/callback");
    }

    private static IResult GoogleMobileLogin(IOptions<SocialAuthSettings> settings)
    {
        var s = settings.Value;
        var redirectUri = Uri.EscapeDataString($"{s.BaseUrl}/auth/google/mobile/callback");
        var scope = Uri.EscapeDataString("openid email profile");

        var url = $"https://accounts.google.com/o/oauth2/v2/auth" +
                  $"?client_id={s.Google.ClientId}" +
                  $"&redirect_uri={redirectUri}" +
                  $"&response_type=code" +
                  $"&scope={scope}" +
                  $"&access_type=offline";

        return Results.Redirect(url);
    }

    private static async Task<IResult> GoogleMobileCallback(
        string code,
        IAuthService authService,
        ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        var idToken = await authService.ExchangeGoogleCodeAsync(code, cancellationToken);
        var account = await authService.LoginWithGoogleAsync(idToken, cancellationToken);
        var jwt = tokenService.GenerateToken(account);

        return Results.Redirect($"careerflowui://auth/callback?token={jwt.Token}");
    }

    private static IResult LinkedInMobileLogin(IOptions<SocialAuthSettings> settings)
    {
        var s = settings.Value;
        var redirectUri = Uri.EscapeDataString($"{s.BaseUrl}/auth/linkedin/mobile/callback");
        var scope = Uri.EscapeDataString("openid profile email");

        var url = $"https://www.linkedin.com/oauth/v2/authorization" +
                  $"?client_id={s.LinkedIn.ClientId}" +
                  $"&redirect_uri={redirectUri}" +
                  $"&response_type=code" +
                  $"&scope={scope}";

        return Results.Redirect(url);
    }

    private static async Task<IResult> LinkedInMobileCallback(
        string code,
        IAuthService authService,
        ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        var account = await authService.LoginWithLinkedInAsync(code, cancellationToken);
        var jwt = tokenService.GenerateToken(account);

        return Results.Redirect($"careerflowui://auth/callback?token={jwt.Token}");
    }
}