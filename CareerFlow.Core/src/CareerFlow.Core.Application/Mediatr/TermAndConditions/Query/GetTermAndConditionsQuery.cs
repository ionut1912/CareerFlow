using CareerFlow.Core.Application.Dtos;
using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.TermAndConditions.Query;

public record GetTermAndConditionsQuery(Guid Id) : IRequest<TermsAndConditionDto>
{
}
