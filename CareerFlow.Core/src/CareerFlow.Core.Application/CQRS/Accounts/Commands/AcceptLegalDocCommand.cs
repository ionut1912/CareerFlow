namespace CareerFlow.Core.Application.CQRS.Accounts.Commands;

public record AcceptLegalDocCommand(Guid AccountId, string Type);