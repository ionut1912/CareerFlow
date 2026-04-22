using JetBrains.Annotations;

namespace CareerFlow.Core.Infrastructure.Configurations;

public class GoogleSettings
{
    public required string ClientId { get; init; }
    [UsedImplicitly(ImplicitUseKindFlags.Assign)]
    public required string ClientSecret { get; init; }
}
