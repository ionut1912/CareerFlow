using System.ComponentModel.DataAnnotations;

namespace CareerFlow.Core.Infrastructure.Configurations;

public sealed class OpenAIOptions
{
    public const string SectionName = "OpenAI";

    [Required] public string ApiKey { get; init; } = default!;

    [Required] public string BaseUrl { get; init; } = "https://api.openai.com/v1";

    public string DefaultModel { get; init; } = "gpt-4o";

    [Range(1, 300)] public int TimeoutSeconds { get; init; } = 30;

    [Range(1, 10)] public int MaxRetries { get; init; } = 3;
}