using JetBrains.Annotations;

namespace CareerFlow.Core.Infrastructure.Configurations;

public class LegalDocSettings
{
    public static string SectionName => "LegalSettings";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)]
    public required string GitHubPagesBaseUrl { get; init; }
}
