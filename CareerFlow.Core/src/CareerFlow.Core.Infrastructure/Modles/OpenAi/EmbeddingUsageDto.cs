namespace CareerFlow.Core.Infrastructure.Modles.OpenAi;

public sealed record EmbeddingUsageDto(int PromptTokens, int TotalTokens);