using CareerFlow.Core.Application.CQRS.Accounts.Commands;
using CareerFlow.Core.Application.CQRS.Accounts.Query;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Shared.Api.Endpoints;
using Shared.Api.Extensions;
using Shared.Api.Infrastructure;
using Wolverine;

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
        group.MapDelete(DeleteUserAccount);
    }

    private static async Task<IResult> Register(IMessageBus bus, CreateAccountRequest createAccountRequest,
        CancellationToken ct)
    {
        var createAccountCommand = createAccountRequest.ToCreateCommand();
        var createdAccount = await bus.InvokeAsync<Guid>(createAccountCommand, ct);
        return Results.Ok(createdAccount);
    }

    private static async Task<IResult> Login(IMessageBus bus, LoginRequest loginRequest,
        CancellationToken ct)
    {
        var loginQuery = loginRequest.ToLoginQuery();
        var result = await bus.InvokeAsync<AccountDto>(loginQuery, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> LoginWithGoogle(IMessageBus bus, GoogleLoginRequest googleLoginRequest,
        CancellationToken ct)
    {
        var googleLoginQuery = googleLoginRequest.ToLoginWithGoogleQuery();
        var result = await bus.InvokeAsync<AccountDto>(googleLoginQuery, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> LoginWithLinkedin(IMessageBus bus, LinkedInLoginRequest linkedinLoginRequest,
        CancellationToken ct)
    {
        var linkedinLoginQuery = linkedinLoginRequest.ToLoginWithLinkedinQuery();
        var result = await bus.InvokeAsync<AccountDto>(linkedinLoginQuery, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> RefreshToken(IMessageBus bus, RefreshTokenRequest refreshTokenRequest, CancellationToken ct)
    {
        var refreshTokenCommand = refreshTokenRequest.ToCreateRefreshTokenCommand();
        var result = await bus.InvokeAsync<AccountDto>(refreshTokenCommand, ct);
        return Results.Ok(result);
    }

    [Authorize]
    private static async Task<IResult> GetCurrentAccount(IMessageBus bus, HttpContext httpContext, CancellationToken ct)
    {
        var accountId = httpContext.GetAccountId();
        if (accountId == Guid.Empty) return Results.Unauthorized();

        var currentUserQuery = new GetCurrentAccountQuery(accountId);
        var result = await bus.InvokeAsync<AccountDto>(currentUserQuery, ct);
        return Results.Ok(result);
    }

    [Authorize]
    private static async Task<IResult> ResetPassword(IMessageBus bus, HttpContext httpContext, ResetPasswordRequest resetPasswordRequest, CancellationToken ct)
    {
        var accountId = httpContext.GetAccountId();
        if (accountId == Guid.Empty) return Results.Unauthorized();
        var resetPasswordCommand = resetPasswordRequest.ToResetPasswordCommand(accountId);
        await bus.InvokeAsync(resetPasswordCommand, ct);
        return Results.NoContent();
    }

    [Authorize]
    private static async Task<IResult> DeleteUserAccount(IMessageBus bus, HttpContext httpContext, CancellationToken ct)
    {
        var accountId = httpContext.GetAccountId();
        if (accountId == Guid.Empty) return Results.Unauthorized();

        var deleteAccountCommand = new DeleteAccountCommand(accountId);
        await bus.InvokeAsync(deleteAccountCommand, ct);
        return Results.NoContent();
    }
}