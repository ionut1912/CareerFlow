using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Application.CQRS.UserProfiles.Queries;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Requests.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Api.Endpoints;
using Shared.Api.Extensions;
using Shared.Api.Infrastructure;
using Wolverine;

namespace CareerFlow.Core.Api.Features.UserProfiles;

public class UserProfileEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(this)
            .RequireAuthorization();
        
        group.MapPost(CreateUserProfile);
        group.MapGet(GetUserProfiles);
        group.MapGet(GetUserProfile, "/{id}");
        group.MapGet(GetCurrentUserProfile, "/current");
        group.MapGet(GetCurrentUserProfileWithCourses,"/current/with-courses");
        group.MapPut(UpdateUserProfile, "/{id}");
        group.MapDelete(DeleteUserProfile, "/{id}");
    }
    
    private static async Task<Results<Ok<Guid>, UnauthorizedHttpResult>> CreateUserProfile(
        IMessageBus messageBus,
        HttpContext httpContext,
        CreateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var accountId = httpContext.GetAccountId();
        if (accountId == Guid.Empty) return TypedResults.Unauthorized();
        var createUserProfileCommand = request.ToCreateUserProfileCommand(accountId);
        var userProfileId = await messageBus.InvokeAsync<Guid>(createUserProfileCommand, cancellationToken);
        return TypedResults.Ok(userProfileId);
    }
    
    private static async Task<Ok<List<UserProfileDto>>> GetUserProfiles(
        IMessageBus messageBus,
        CancellationToken cancellationToken)
    {
        var query = new GetUserProfilesQuery();
        var result = await messageBus.InvokeAsync<List<UserProfileDto>>(query, cancellationToken);
        return TypedResults.Ok(result);
    }
    
    private static async Task<Ok<UserProfileDto>> GetUserProfile(
        IMessageBus messageBus,
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetUserProfileByIdQuery(id);
        var result = await messageBus.InvokeAsync<UserProfileDto>(query, cancellationToken);
        return TypedResults.Ok(result);
    }
    
    private static async Task<Results<Ok<UserProfileDto>, UnauthorizedHttpResult>> GetCurrentUserProfile(
        IMessageBus messageBus,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var accountId = httpContext.GetAccountId();
        if (accountId == Guid.Empty) return TypedResults.Unauthorized();
        var query = new GetCurrentUserProfileQuery(accountId);
        var result = await messageBus.InvokeAsync<UserProfileDto>(query, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static async Task<NoContent> UpdateUserProfile(
        IMessageBus messageBus,
        Guid id,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        var updateCommand = request.ToUpdateUserProfileCommand(id);
        await messageBus.InvokeAsync(updateCommand, cancellationToken);
        return TypedResults.NoContent();
    }
    
    private static async Task<NoContent> DeleteUserProfile(
        IMessageBus messageBus,
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleteCommand = new DeleteUserProfileCommand(id);
        await messageBus.InvokeAsync(deleteCommand, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<UserProfileDto>, UnauthorizedHttpResult>> GetCurrentUserProfileWithCourses(
        IMessageBus messageBus, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var accountId = httpContext.GetAccountId();
        if (accountId == Guid.Empty) return TypedResults.Unauthorized();
        var query = new GetCurrentUserCoursesQuery(accountId);
        var result =await messageBus.InvokeAsync<UserProfileDto>(query, cancellationToken);
        return TypedResults.Ok(result);
    }
}