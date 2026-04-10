namespace CareerFlow.Core.Application.CQRS.Accounts.Queries;

public sealed record GoogleMobileCallbackQuery(string Code,string State);