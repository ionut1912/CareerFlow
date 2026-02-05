using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class AccountMapping
{
    public static AccountDto ToDto(this Account account, string? token)
    {
        return new AccountDto(account.Id, account.Email, account.Username, token, account.IsFounder);
    }

    public static List<AccountDto> ToDtoList(this IEnumerable<Account> accounts) => [.. accounts.Select(a => new AccountDto(a.Id, a.Email, a.Username, null, a.IsFounder))];
}
