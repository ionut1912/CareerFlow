using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Api.Endpoints;
using Shared.Api.Infrastructure;
using Wolverine;

namespace CareerFlow.Core.Api.Features.Account;

[UsedImplicitly]
public class SocialEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup(this);
        group.MapGet("/auth/google/mobile", GoogleMobileLogin);
        group.MapGet("/auth/google/mobile/callback", GoogleMobileCallback);
        group.MapGet("/auth/linkedin/mobile", LinkedInMobileLogin);
        group.MapGet("/auth/linkedin/mobile/callback", LinkedInMobileCallback);
    }

    private static async Task<RedirectHttpResult> GoogleMobileLogin(
        [AsParameters] GoogleMobileLoginQuery request,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        string url = await messageBus.InvokeAsync<string>(request, ct);
        return TypedResults.Redirect(url);
    }

    private static async Task<RedirectHttpResult> GoogleMobileCallback(
        [AsParameters] GoogleMobileCallbackQuery query,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        string url = await messageBus.InvokeAsync<string>(query, cancellationToken);
        return TypedResults.Redirect(url);
    }

    private static async Task<RedirectHttpResult> LinkedInMobileLogin(
        [AsParameters] LinkedinMobileLoginQuery query,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        string url = await messageBus.InvokeAsync<string>(query, ct);
        return TypedResults.Redirect(url);
    }

    private static async Task<RedirectHttpResult> LinkedInMobileCallback(
        [AsParameters] LinkedInMobileCallbackQuery query,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        string url = await messageBus.InvokeAsync<string>(query, cancellationToken);
        return TypedResults.Redirect(url);
    }
}
