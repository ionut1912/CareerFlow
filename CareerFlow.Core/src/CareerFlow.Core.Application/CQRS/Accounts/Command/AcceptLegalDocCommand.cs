namespace CareerFlow.Core.Application.CQRS.Accounts.Command;

public record AcceptLegalDocCommand(Guid AccountId, string Type);