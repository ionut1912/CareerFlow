using CareerFlow.Core.Application.CQRS.Accounts.Commands;
using CareerFlow.Core.Application.CQRS.Accounts.Queries;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests.Account;
using CareerFlow.Core.Application.Requests.LegalDoc;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class AccountMapping
{
    public static AccountDto ToAccountDto(this Account account, string? token = null, string? refreshToken = null) =>
        new(account.Id, account.Email, account.Username, account.Name, token, refreshToken,
            account.IsFounder,
            account.PrivacyPolicyAccepted, account.TermsAccepted);

    public static CreateAccountCommand ToCreateCommand(this CreateAccountRequest request) =>
        new(request.Email, request.Password, request.ConfirmPassword, request.Username,
            request.Name);

    public static ForgotPasswordCommand ToForgotPasswordCommand(this ForgotPasswordRequest request, string link,
        string token) =>
        new(request.Email, link, token);

    public static LoginQuery ToLoginQuery(this LoginRequest request) => new(request.Email, request.Password);

    public static ResetPasswordCommand ToResetPasswordCommand(this ResetPasswordRequest request) =>
        new(request.Email, request.NewPassword, request.Token);

    public static CreateRefreshTokenCommand ToCreateRefreshTokenCommand(this RefreshTokenRequest request) =>
        new(request.Token, request.RefreshToken);

    public static AcceptLegalDocCommand ToAcceptLegalDocCommand(this AcceptLegalDocRequest request, Guid accountId) =>
        new(accountId, request.Type);
}
