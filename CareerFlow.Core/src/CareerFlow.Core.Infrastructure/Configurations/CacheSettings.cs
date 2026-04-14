namespace CareerFlow.Core.Infrastructure.Configurations;

public class CacheSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; init; } = string.Empty;
    public string InstanceName { get; init; } = "MyApp:";
    public int DefaultExpiryMinutes { get; init; } = 60;
    public bool AbortOnConnectFail { get; init; } = false;
}