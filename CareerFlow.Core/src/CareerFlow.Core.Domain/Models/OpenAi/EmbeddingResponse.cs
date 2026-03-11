namespace CareerFlow.Core.Domain.Models.OpenAi;

public sealed record EmbeddingResponse(
    List<EmbeddingDataDto> Data,
    EmbeddingUsageDto Usage
);
