using CareerFlow.Core.Application.Dtos;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Commands;

public record UpdatePrivacyPolicyCommand(Guid Id, string Content) : IRequest<PrivacyPolicyDto>
{
}
