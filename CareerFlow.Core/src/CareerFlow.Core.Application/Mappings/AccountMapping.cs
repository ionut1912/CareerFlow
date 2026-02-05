using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class AccountMapping
{
    public static AccountDto ToDto(this Account account, string? token)
    {
        return new AccountDto(account.Email, account.Username, token, account.IsFounder);
    }
}
