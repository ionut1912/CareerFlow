using CareerFlow.Core.Infrastructure.Modles.OpenAi;

namespace CareerFlow.Core.Infrastructure.OpenAIAbstractions;

public interface IAIEmbeddingService
{
    Task<EmbeddingResult> GetEmbeddingAsync(
        string text,
        CancellationToken ct = default);
}