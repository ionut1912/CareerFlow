namespace CareerFlow.Core.Infrastructure.Configurations;

public class CacheSettings
{
    public const string SectionName = "Redis";

    public required string ConnectionString { get; init; }
    public required string InstanceName { get; init; } = "careerFlow:";
    public required int DefaultExpiryMinutes { get; init; } = 60;
    public required bool AbortOnConnectFail { get; init; }
}
