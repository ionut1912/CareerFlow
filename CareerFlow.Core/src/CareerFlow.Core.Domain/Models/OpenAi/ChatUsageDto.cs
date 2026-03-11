namespace CareerFlow.Core.Domain.Models.OpenAi;

public sealed record ChatUsageDto(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens
);