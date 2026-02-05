using CareerFlow.Core.Application.Dtos;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Query;

public record LoginQuery(string Username, string Password) : IRequest<AccountDto>
{

}