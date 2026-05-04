using JetBrains.Annotations;

namespace CareerFlow.Core.Application.CQRS.Accounts.Queries;

[UsedImplicitly]
public sealed record LinkedInMobileCallbackQuery(string Code, string State);
