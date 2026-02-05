using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Commands;

public record CreatePrivacyPolicyCommand(string Content) : IRequest<Guid>
{
}
