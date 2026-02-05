using CareerFlow.Core.Application.Dtos;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Query;

public record GetPrivacyPolicyQuery(Guid Id) : IRequest<PrivacyPolicyDto>
{
}
