using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Application.CQRS.UserProfiles.Queries;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Shared.Api.Endpoints;
using Shared.Api.Extensions;
using Shared.Api.Infrastructure;
using Wolverine;

namespace CareerFlow.Core.Api.Endpoints;

public class UserProfileEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(this);
        group.MapPost(CreateUserProfile);
        group.MapGet(GetUserProfiles);
        group.MapGet(GetUserProfile, "/{id}");
        group.MapGet(GetCurrentUserProfile, "/current");
        group.MapPut(UpdateUserProfile, "/{id}");
        group.MapDelete(DeleteUserProfile, "/{id}");
    }

    [Authorize]
    private static async Task<IResult> CreateUserProfile(IMessageBus messageBus, HttpContext httpContext,
        CreateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var accountId = httpContext.GetAccountId();
        if (accountId == Guid.Empty) return Results.Unauthorized();
        var createUserProfileCommand = request.ToCreateUserProfileCommand(accountId);
        var userProfileId = await messageBus.InvokeAsync<Guid>(createUserProfileCommand, cancellationToken);
        return Results.Ok(userProfileId);
    }

    [Authorize]
    private static async Task<IResult> GetUserProfiles(IMessageBus messageBus, CancellationToken cancellationToken)
    {
        var query = new GetUserProfilesQuery();
        var result = await messageBus.InvokeAsync<List<UserProfileDto>>(query, cancellationToken);
        return Results.Ok(result);
    }

    [Authorize]
    private static async Task<IResult> GetUserProfile(IMessageBus messageBus, Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetUserProfileByIdQuery(id);
        var result = await messageBus.InvokeAsync<UserProfileDto>(query, cancellationToken);
        return Results.Ok(result);
    }

    [Authorize]
    private static async Task<IResult> GetCurrentUserProfile(IMessageBus messageBus, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var accountId = httpContext.GetAccountId();
        if (accountId == Guid.Empty) return Results.Unauthorized();
        var query = new GetCurrentUserProfileQuery(accountId);
        var result = await messageBus.InvokeAsync<UserProfileDto>(query, cancellationToken);
        return Results.Ok(result);
    }

    [Authorize]
    private static async Task<IResult> UpdateUserProfile(IMessageBus messageBus, Guid id,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var updateCommand = request.ToUpdateUserProfileCommand(id);
        await messageBus.InvokeAsync(updateCommand, cancellationToken);
        return Results.NoContent();
    }

    [Authorize]
    private static async Task<IResult> DeleteUserProfile(IMessageBus messageBus, Guid id,
        CancellationToken cancellationToken)
    {
        var deleteCommand = new DeleteUserProfileCommand(id);
        await messageBus.InvokeAsync(deleteCommand, cancellationToken);
        return Results.NoContent();
    }
}