using CareerFlow.Core.Application.Dtos;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Query
{
    public record LoginWithGoogleQuery(string IdToken) : IRequest<AccountDto>
    {
    }
}
