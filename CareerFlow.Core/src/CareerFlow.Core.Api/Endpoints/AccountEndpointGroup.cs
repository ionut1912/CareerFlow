using CareerFlow.Core.Application.CQRS.Accounts.Command;
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
        group.MapPost(RefreshToken, "/refresh-token");
        group.MapPost(ForgotPassword, "/forgot-password");
        group.MapPut(ResetPassword, "/reset-password");
        group.MapGet(GetCurrentAccount, "/current");
        group.MapDelete(DeleteUserAccount);
    }

    private static async Task<IResult> Register(IMessageBus bus, CreateAccountRequest createAccountRequest,
        CancellationToken cancellationToken)
    {
        var createAccountCommand = createAccountRequest.ToCreateCommand();
        var createdAccount = await bus.InvokeAsync<Guid>(createAccountCommand, cancellationToken);
        return Results.Ok(createdAccount);
    }

    private static async Task<IResult> Login(IMessageBus bus, LoginRequest loginRequest,
        CancellationToken cancellationToken)
    {
        var loginQuery = loginRequest.ToLoginQuery();
        var result = await bus.InvokeAsync<AccountDto>(loginQuery, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> ForgotPassword(IMessageBus bus, ForgotPasswordRequest forgotPasswordRequest,
        CancellationToken cancellationToken)
    {
        var resetToken = Guid.NewGuid().ToString();
        var resetPasswordLink = "https://carerflow-api.ro/reset-password";
        var finalLink = $"{resetPasswordLink}?token={resetToken}";
        var command = forgotPasswordRequest.ToForgotPasswordCommand(finalLink,resetToken);
        await bus.InvokeAsync(command, cancellationToken);
        return Results.NoContent();
    }
    
    private static async Task<IResult> ResetPassword(IMessageBus bus, HttpContext httpContext,
        ResetPasswordRequest resetPasswordRequest, CancellationToken cancellationToken)
    {
        var resetPasswordCommand = resetPasswordRequest.ToResetPasswordCommand();
        await bus.InvokeAsync(resetPasswordCommand, cancellationToken);
        return Results.NoContent();
    }
    
    [Authorize]
    private static async Task<IResult> RefreshToken(IMessageBus bus, RefreshTokenRequest refreshTokenRequest,
        CancellationToken cancellationToken)
    {
        var refreshTokenCommand = refreshTokenRequest.ToCreateRefreshTokenCommand();
        var result = await bus.InvokeAsync<RefreshTokenDto>(refreshTokenCommand, cancellationToken);
        return Results.Ok(result);
    }

    [Authorize]
    private static async Task<IResult> GetCurrentAccount(IMessageBus bus, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var accountId = httpContext.GetAccountId();
        if (accountId == Guid.Empty) return Results.Unauthorized();

        var currentUserQuery = new GetCurrentAccountQuery(accountId);
        var result = await bus.InvokeAsync<AccountDto>(currentUserQuery, cancellationToken);
        return Results.Ok(result);
    }
    


    [Authorize]
    private static async Task<IResult> DeleteUserAccount(IMessageBus bus, HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var accountId = httpContext.GetAccountId();
        if (accountId == Guid.Empty) return Results.Unauthorized();

        var deleteAccountCommand = new DeleteAccountCommand(accountId);
        await bus.InvokeAsync(deleteAccountCommand, cancellationToken);
        return Results.NoContent();
    }
}