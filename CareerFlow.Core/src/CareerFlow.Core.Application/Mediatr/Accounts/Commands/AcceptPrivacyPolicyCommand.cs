using CareerFlow.Core.Application.Dtos;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Commands;

public record AcceptPrivacyPolicyCommand(Guid Id) : IRequest<AccountDto>
{
}
