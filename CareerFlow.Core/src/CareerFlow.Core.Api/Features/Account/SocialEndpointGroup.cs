using CareerFlow.Core.Application.CQRS.Accounts.Queries;
<<<<<<< HEAD
using CareerFlow.Core.Domain.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
=======
>>>>>>> master
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

    private static async Task<IResult> GoogleMobileLogin(
        [AsParameters] GoogleMobileLoginQuery request,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var url = await messageBus.InvokeAsync<string>(request, ct);
        return Results.Redirect(url);
    }

    private static async Task<IResult> GoogleMobileCallback(
        [AsParameters] GoogleMobileCallbackQuery query,
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var url = await messageBus.InvokeAsync<string>(query, cancellationToken);
        return Results.Redirect(url);
    }

    private static async Task<IResult> LinkedInMobileLogin(
        [AsParameters] LinkedinMobileLoginQuery query,
        IMessageBus messageBus,
        CancellationToken ct)
    {
        var url = await messageBus.InvokeAsync<string>(query, ct);
        return Results.Redirect(url);
    }

    private static async Task<IResult> LinkedInMobileCallback(
<<<<<<< HEAD
        [AsParameters]  LinkedInMobileCallbackQuery query,
=======
        [AsParameters] LinkedInMobileCallbackQuery query,
>>>>>>> master
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var url = await messageBus.InvokeAsync<string>(query, cancellationToken);
        return Results.Redirect(url);
    }
}