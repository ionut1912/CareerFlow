using CareerFlow.Core.Domain.Abstractions.Services;
using Shared.Api.Endpoints;
using Shared.Api.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CareerFlow.Core.Api.Features.Account;

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

    // Am adăugat parametrul returnUrl pe care îl primește de la aplicația ta Expo
    private static IResult GoogleMobileLogin(ISocialService service, string? returnUrl = null)
    {
        var url = service.GoogleMobileLogin(returnUrl);
        return Results.Redirect(url);
    }

    private static async Task<IResult> GoogleMobileCallback(
        string code,
        string state,
        ISocialService service,
        CancellationToken cancellationToken)
    {
        var url = await service.GoogleMobileCallBack(code, state, cancellationToken);
        return Results.Redirect(url);
    }

    // Am adăugat parametrul returnUrl și aici
    private static IResult LinkedInMobileLogin(ISocialService service, string? returnUrl = null)
    {
        var url = service.LinkedInMobileLogin(returnUrl);
        return Results.Redirect(url);
    }

    private static async Task<IResult> LinkedInMobileCallback(
        string code,
        string state,
        ISocialService service,
        CancellationToken cancellationToken)
    {
        var url = await service.LinkedInCallBack(code, state, cancellationToken);
        return Results.Redirect(url);
    }
}