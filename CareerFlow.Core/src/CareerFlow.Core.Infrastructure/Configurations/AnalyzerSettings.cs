namespace CareerFlow.Core.Infrastructure.Configurations;

public sealed class AnalyzerSettings
{
    public const string SectionName = "Analyzer";
    public string BaseUrl { get; init; } = string.Empty;
    public int TimeoutSec { get; init; } = 120;
}