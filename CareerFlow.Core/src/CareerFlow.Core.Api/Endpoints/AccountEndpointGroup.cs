using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.Accounts.Commands;
using CareerFlow.Core.Application.Mediatr.Accounts.Query;
using CareerFlow.Core.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Shared.Api.Endpoints;
using Shared.Api.Extensions;
using Shared.Api.Infrastructure;
using Shared.Application.Mediator;
using System.Security.Claims;

namespace CareerFlow.Core.Api.Endpoints;

public class AccountEndpointGroup : EndpointGroup
{
    public override void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup(this);
        group.MapPost(Register, "/register");
        group.MapPost(Login, "/login");
        group.MapPost(LoginWithGoogle, "/google");
        group.MapPost(LoginWithLinkedin, "/linkedin");
        group.MapPost(RefreshToken, "/refresh-token");
        group.MapPost(ResetPassword, "/reset-password");
        group.MapGet(GetCurrentAccount, "/current");
        group.MapGet(GetAllAccounts);
        group.MapDelete(DeleteUserAccount, "/");
    }

    private static async Task<IResult> Register(IMediator mediator, CreateAccountRequest createAccountRequest,
        CancellationToken ct)
    {
        var createAccountCommand = createAccountRequest.ToCreateCommand();
        var createdAccount = await mediator.Send(createAccountCommand, ct);
        return Results.Ok(createdAccount);
    }

    private static async Task<IResult> Login(IMediator mediator, LoginRequest loginRequset,
        CancellationToken ct)
    {
        var loginQuery = loginRequset.ToLoginQuery();
        var result = await mediator.Send(loginQuery, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> LoginWithGoogle(IMediator mediator, GoogleLoginRequest googleLoginRequest,
        CancellationToken ct)
    {
        var googleLoginQuery = googleLoginRequest.ToLoginWithGoogleQuery();
        var result = await mediator.Send(googleLoginQuery, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> LoginWithLinkedin(IMediator mediator, LinkedInLoginRequest linkedinLoginRequest,
        CancellationToken ct)
    {
        var linkedinLoginQuery = linkedinLoginRequest.ToLoginWithLinkedinQuery();
        var result = await mediator.Send(linkedinLoginQuery, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetAllAccounts(IMediator mediator, CancellationToken ct)
    {
        var getAllAcountsQuery = new GetAllAcountsQuery();
        var result = await mediator.Send(getAllAcountsQuery, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> RefreshToken(IMediator mediator, RefreshTokenRequest refreshTokenRequest, CancellationToken ct)
    {
        var refreshTokenCommand = refreshTokenRequest.ToCreateRefreshTokenCommand();
        var result = await mediator.Send(refreshTokenCommand, ct);
        return Results.Ok(result);
    }

    [Authorize]
    private static async Task<IResult> GetCurrentAccount(IMediator mediator, HttpContext httpContext, CancellationToken ct)
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