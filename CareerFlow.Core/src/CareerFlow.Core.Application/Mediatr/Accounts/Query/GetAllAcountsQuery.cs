using CareerFlow.Core.Application.Dtos;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Query
{
    public class GetAllAcountsQuery : IRequest<List<AccountDto>>
    {
    }
}
