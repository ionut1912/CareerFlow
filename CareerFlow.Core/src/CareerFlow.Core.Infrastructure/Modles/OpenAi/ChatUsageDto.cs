namespace CareerFlow.Core.Infrastructure.Modles.OpenAi;

public sealed record ChatUsageDto(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens
);