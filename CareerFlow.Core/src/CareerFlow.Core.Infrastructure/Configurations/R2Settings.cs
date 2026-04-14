namespace CareerFlow.Core.Infrastructure.Configurations;

public sealed class R2Settings
{
    public const string SectionName = "R2";

    public string AccountId { get; init; } = string.Empty;
    public string AccessKey { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public string BucketName { get; init; } = string.Empty;

    public string Endpoint => $"https://{AccountId}.r2.cloudflarestorage.com";
}