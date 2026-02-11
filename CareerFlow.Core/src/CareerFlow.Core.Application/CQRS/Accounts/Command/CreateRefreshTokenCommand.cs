namespace CareerFlow.Core.Application.CQRS.Accounts.Command;

public record CreateRefreshTokenCommand(string Token, string RefreshToken);
