namespace CareerFlow.Core.Application.CQRS.Accounts.Query;

public record LoginQuery(string Email, string Password);