namespace CareerFlow.Core.Application.CQRS.Accounts.Commands;

public record ResetPasswordCommand(string Email, string NewPassword, string Token);
