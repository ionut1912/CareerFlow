using Shared.Application.Mediator;


namespace CareerFlow.Core.Application.Mediatr.TermAndConditions.Commands;

public record CreateTermAndConditionsCommand(string Content) : IRequest<Guid>;
