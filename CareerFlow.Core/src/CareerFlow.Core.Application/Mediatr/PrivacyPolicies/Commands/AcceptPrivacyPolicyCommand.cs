using CareerFlow.Core.Application.Dtos;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Commands;

public record AcceptPrivacyPolicyCommand(Guid Id): IRequest<PrivacyPolicyDto>
{
}
