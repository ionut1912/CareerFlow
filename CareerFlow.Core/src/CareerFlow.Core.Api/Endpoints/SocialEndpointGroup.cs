using CareerFlow.Core.Domain.Abstractions.Services;
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

    private static IResult GoogleMobileLogin(ISocialService service)
    {
        var url = service.GoogleMobileLogin();

        return Results.Redirect(url);
    }

    private static async Task<IResult> GoogleMobileCallback(
        string code,
        ISocialService service,
        CancellationToken cancellationToken)
    {
        var url = await service.GoogleMobileCallBack(code, cancellationToken);

        return Results.Redirect(url);
    }

    private static IResult LinkedInMobileLogin(ISocialService service)
    {
        var url = service.LinkedInMobileLogin();

        return Results.Redirect(url);
    }

    private static async Task<IResult> LinkedInMobileCallback(
        string code,
        ISocialService service,
        CancellationToken cancellationToken)
    {
        var url = await service.LinkedInCallBack(code, cancellationToken);
        return Results.Redirect(url);
    }
}