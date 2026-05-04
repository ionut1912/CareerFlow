namespace CareerFlow.Core.Infrastructure.Configurations;

public sealed class R2Settings
{
    public const string SectionName = "R2";

    public required string AccountId { get; init; }
    public required string AccessKey { get; init; }
    public required string SecretKey { get; init; }
    public required string BucketName { get; init; }

    public string Endpoint => $"https://{AccountId}.r2.cloudflarestorage.com";
}
