using CareerFlow.Core.Application.Dtos;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Query;

public record GetCurrentAccountQuery(string Username) : IRequest<AccountDto>
{

}