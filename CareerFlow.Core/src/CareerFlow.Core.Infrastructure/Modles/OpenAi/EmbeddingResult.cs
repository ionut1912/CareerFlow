namespace CareerFlow.Core.Infrastructure.Modles.OpenAi;

public record EmbeddingResult(
    float[] Vector,
    int TokensUsed
);