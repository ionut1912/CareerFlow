using JetBrains.Annotations;

namespace CareerFlow.Core.Application.CQRS.Accounts.Queries;

[UsedImplicitly]
public sealed record GoogleMobileCallbackQuery(string Code, string State);
