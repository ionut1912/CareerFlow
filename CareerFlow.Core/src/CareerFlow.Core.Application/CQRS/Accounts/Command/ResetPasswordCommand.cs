namespace CareerFlow.Core.Application.CQRS.Accounts.Commands;

public record ResetPasswordCommand(Guid AccountId, string NewPassword);
