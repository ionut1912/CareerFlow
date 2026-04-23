using JetBrains.Annotations;

namespace CareerFlow.Core.Infrastructure.Configurations;

public class PostmarkSettings
{
    public const string SectionName = "PostmarkSettings";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)]
    public required string ServerToken { get; init; }

    [UsedImplicitly(ImplicitUseKindFlags.Assign)]
    public required string FromAddress { get; init; }
}
