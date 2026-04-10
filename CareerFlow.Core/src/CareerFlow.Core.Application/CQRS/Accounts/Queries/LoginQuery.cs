namespace CareerFlow.Core.Application.CQRS.Accounts.Queries;

public record LoginQuery(string Email, string Password);