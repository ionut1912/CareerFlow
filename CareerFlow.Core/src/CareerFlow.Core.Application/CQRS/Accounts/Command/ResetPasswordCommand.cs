namespace CareerFlow.Core.Application.CQRS.Accounts.Command;

public record ResetPasswordCommand(Guid AccountId, string NewPassword);
