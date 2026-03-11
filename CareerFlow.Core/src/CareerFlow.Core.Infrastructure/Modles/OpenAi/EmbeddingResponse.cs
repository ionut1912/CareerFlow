namespace CareerFlow.Core.Infrastructure.Modles.OpenAi;

public sealed record EmbeddingResponse(
    List<EmbeddingDataDto> Data,
    EmbeddingUsageDto Usage
);