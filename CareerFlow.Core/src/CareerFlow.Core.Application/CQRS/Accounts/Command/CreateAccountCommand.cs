namespace CareerFlow.Core.Application.CQRS.Accounts.Command;

public record CreateAccountCommand(string Email, string Password, string Username, bool AcceptedPrivacyPolicy, bool AcceptedTermsAndConditions);