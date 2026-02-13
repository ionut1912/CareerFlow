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
        return new AccountDto(account.Id, account.Email, account.Username, token, refreshToken, account.IsFounder, account.PrivacyPolicyAccepted, account.TermsAccepted);
    }

    public static List<AccountDto> ToAccountsDto(this IEnumerable<Account> accounts) => [.. accounts.Select(a => new AccountDto(a.Id, a.Email, a.Username, null, null, a.IsFounder, a.PrivacyPolicyAccepted, a.TermsAccepted))];

    public static CreateAccountCommand ToCreateCommand(this CreateAccountRequest request)
    {
        return new CreateAccountCommand(request.Email, request.Password, request.Username,request.Name, request.AcceptedPrivacyPolicy, request.AcceptedTermsAndConditions);
    }

    public static LoginQuery ToLoginQuery(this LoginRequest request)
    {
        return new LoginQuery(request.Email, request.Password);
    }

    public static ResetPasswordCommand ToResetPasswordCommand(this ResetPasswordRequest request, Guid accountId)
    {
        return new ResetPasswordCommand(accountId, request.NewPassword);
    }

    public static CreateRefreshTokenCommand ToCreateRefreshTokenCommand(this RefreshTokenRequest request)
    {
        return new CreateRefreshTokenCommand(request.Token, request.RefreshToken);
    }

    public static LoginWithGoogleQuery ToLoginWithGoogleQuery(this GoogleLoginRequest request)
    {
        return new LoginWithGoogleQuery(request.IdToken);
    }

    public static LoginWithLinkedinQuery ToLoginWithLinkedinQuery(this LinkedInLoginRequest request)
    {
        return new LoginWithLinkedinQuery(request.AuthorizationCode);
    }
}
