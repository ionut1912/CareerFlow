using CareerFlow.Core.Application.CQRS.Accounts.Command;
using CareerFlow.Core.Application.CQRS.Accounts.Query;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class AccountMapping
{
    public static AccountDto ToAccountDto(this Account account, string? token = null, string? refreshToken = null)
    {
        return new AccountDto(account.Id, account.Email, account.Username, account.Name, token, refreshToken,
            account.IsFounder,
            account.PrivacyPolicyAccepted, account.TermsAccepted);
    }

    public static CreateAccountCommand ToCreateCommand(this CreateAccountRequest request)
    {
        return new CreateAccountCommand(request.Email, request.Password, request.ConfirmPassword, request.Username,
            request.Name);
    }

    public static ForgotPasswordCommand ToForgotPasswordCommand(this ForgotPasswordRequest request, string link,
        string token)
    {
        return new ForgotPasswordCommand(request.Email, link, token);
    }

    public static LoginQuery ToLoginQuery(this LoginRequest request)
    {
        return new LoginQuery(request.Email, request.Password);
    }

    public static ResetPasswordCommand ToResetPasswordCommand(this ResetPasswordRequest request)
    {
        return new ResetPasswordCommand(request.Email, request.NewPassword, request.Token);
    }

    public static CreateRefreshTokenCommand ToCreateRefreshTokenCommand(this RefreshTokenRequest request)
    {
        return new CreateRefreshTokenCommand(request.Token, request.RefreshToken);
    }

    public static AcceptLegalDocCommand ToAcceptLegalDocCommand(this AcceptLegalDocRequest request, Guid accountId)
    {
        return new AcceptLegalDocCommand(accountId, request.Type);
    }
}