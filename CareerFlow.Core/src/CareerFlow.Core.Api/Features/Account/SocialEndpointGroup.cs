using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Api.Endpoints;
using Shared.Api.Infrastructure;
using Wolverine;

namespace CareerFlow.Core.Api.Features.Account;

public class SocialEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(this);
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
        var url = await messageBus.InvokeAsync<string>(request, ct);
        return TypedResults.Redirect(url);
    }

    private static async Task<RedirectHttpResult> GoogleMobileCallback(
        [AsParameters] GoogleMobileCallbackQuery query,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var url = await messageBus.InvokeAsync<string>(query, cancellationToken);
        return TypedResults.Redirect(url);
    }

    private static async Task<RedirectHttpResult> LinkedInMobileLogin(
        [AsParameters] LinkedinMobileLoginQuery query,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var url = await messageBus.InvokeAsync<string>(query, ct);
        return TypedResults.Redirect(url);
    }

    private static async Task<RedirectHttpResult> LinkedInMobileCallback(
        [AsParameters] LinkedInMobileCallbackQuery query,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var url = await messageBus.InvokeAsync<string>(query, cancellationToken);
        return TypedResults.Redirect(url);
    }
}