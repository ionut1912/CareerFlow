namespace CareerFlow.Core.Application.CQRS.Accounts.Commands;

public record CreateRefreshTokenCommand(string Token, string RefreshToken);
