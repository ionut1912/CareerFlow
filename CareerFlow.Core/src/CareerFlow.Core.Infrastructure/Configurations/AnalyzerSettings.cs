using JetBrains.Annotations;

namespace CareerFlow.Core.Infrastructure.Configurations;

public sealed class AnalyzerSettings
{
    public const string SectionName = "Analyzer";

    [UsedImplicitly(ImplicitUseKindFlags.Assign)]
    public required string BaseUrl { get; init; }

    public required int TimeoutSec { get; init; } = 60;
}
