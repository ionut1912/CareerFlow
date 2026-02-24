namespace CareerFlow.Core.Application.CQRS.Accounts.Command;

public record ForgotPasswordCommand(string Email, string ResetPasswordLink,string Token);
