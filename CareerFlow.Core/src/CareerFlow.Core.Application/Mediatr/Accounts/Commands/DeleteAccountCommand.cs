using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Commands;

public record DeleteAccountCommand(Guid Id) : IRequest<Unit>
{

}