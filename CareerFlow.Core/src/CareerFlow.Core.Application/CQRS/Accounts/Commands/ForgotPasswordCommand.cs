namespace CareerFlow.Core.Application.CQRS.Accounts.Commands;

public record ForgotPasswordCommand(string Email, string ResetPasswordLink, string Token);