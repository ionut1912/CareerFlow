using CareerFlow.Core.Api.Mappings;
using CareerFlow.Core.Api.Requests;
using CareerFlow.Core.Application.Mediatr.Accounts.Commands;
using CareerFlow.Core.Application.Mediatr.Accounts.Query;
using Microsoft.AspNetCore.Authorization;
using Shared.Api.Endpoints;
using Shared.Api.Extensions;
using Shared.Api.Infrastructure;
using Shared.Application.Mediator;
using System.Security.Claims;

namespace CareerFlow.Core.Api.Endpoints;

public class UserEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(this);
        group.MapPost(RegisterUser, "/register");
        group.MapPost(LoginUser, "/login");
        group.MapPost(ResetPassword, "/reset-password");
        group.MapGet(GetCurrentUser, "/current");
        group.MapDelete(DeleteUserAccount, "/");
    }

    private static async Task<IResult> RegisterUser(IMediator mediator, CreateAccountRequest createAccountRequest,
        CancellationToken ct)
    {
        var createAccountCommand = createAccountRequest.ToCreateCommand();
        var createdAccount = await mediator.Send(createAccountCommand, ct);
        return Results.Ok(createdAccount);
    }

    private static async Task<IResult> LoginUser(IMediator mediator, LoginRequest loginRequset,
        CancellationToken ct)
    {
        var loginQuery = loginRequset.ToLoginQuery();
        var result = await mediator.Send(loginQuery, ct);
        return Results.Ok(result);
    }

    [Authorize]
    private static async Task<IResult> GetCurrentUser(IMediator mediator, HttpContext httpContext, CancellationToken ct)
    {
        var username = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(username)) return Results.Unauthorized();

        var currentUserQuery = new GetCurrentAccountQuery(username);
        var result = await mediator.Send(currentUserQuery, ct);
        return Results.Ok(result);
    }

    [Authorize]
    private static async Task<IResult> ResetPassword(IMediator mediator, HttpContext httpContext, ResetPasswordRequest resetPasswordRequest, CancellationToken ct)
    {
        var username = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(username)) return Results.Unauthorized();
        var resetPasswordCommand = resetPasswordRequest.ToResetPasswordCommand(username);
        await mediator.Send(resetPasswordCommand, ct);
        return Results.NoContent();
    }

    [Authorize]
    private static async Task<IResult> DeleteUserAccount(IMediator mediator, HttpContext httpContext, CancellationToken ct)
    {
        var accountId = httpContext.GetAccountId();
        if (accountId == Guid.Empty) return Results.Unauthorized();

        var deleteAccountCommand = new DeleteAccountCommand(accountId);
        await mediator.Send(deleteAccountCommand, ct);
        return Results.NoContent();
    }
}