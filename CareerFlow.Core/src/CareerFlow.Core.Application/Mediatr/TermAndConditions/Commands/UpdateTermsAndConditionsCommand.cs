using CareerFlow.Core.Application.Dtos;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.TermAndConditions.Commands;

public record UpdateTermsAndConditionsCommand(string Content) : IRequest<TermsAndConditionDto>
{
}
