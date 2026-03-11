namespace CareerFlow.Core.Domain.Models.OpenAi;

public sealed record EmbeddingUsageDto(int PromptTokens, int TotalTokens);