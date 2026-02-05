using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class AccountMapping
{
    public static AccountDto ToAccountDto(this Account account, string? token)
    {
        return new AccountDto(account.Id, account.Email, account.Username, token, account.IsFounder, account.PrivacyPolicyAccepted, account.TermsAccepted);
    }

    public static List<AccountDto> ToAccountsDto(this IEnumerable<Account> accounts) => [.. accounts.Select(a => new AccountDto(a.Id, a.Email, a.Username, null, a.IsFounder, a.PrivacyPolicyAccepted, a.TermsAccepted))];
}
