namespace CareerFlow.Core.Application.CQRS.Accounts.Command;

public record ResetPasswordCommand(string Email, string NewPassword,string Token);