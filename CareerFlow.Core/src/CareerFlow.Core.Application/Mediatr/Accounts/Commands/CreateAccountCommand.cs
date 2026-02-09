using Shared.Application.Mediator;

namespace CareerFlow.Core.Application.Mediatr.Accounts.Commands;

public record CreateAccountCommand(string Email, string Password, string Username, bool AcceptedPrivacyPolicy, bool AcceptedTermsAndConditions) : IRequest<Guid>;