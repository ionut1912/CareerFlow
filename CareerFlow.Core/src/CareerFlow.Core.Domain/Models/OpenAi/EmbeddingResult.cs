namespace CareerFlow.Core.Domain.Models.OpenAi;

public record EmbeddingResult(
    float[] Vector,
    int TokensUsed
);