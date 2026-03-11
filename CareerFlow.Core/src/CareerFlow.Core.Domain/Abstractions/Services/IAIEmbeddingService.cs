using CareerFlow.Core.Domain.Models.OpenAi;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IAIEmbeddingService
{
    Task<EmbeddingResult> GetEmbeddingAsync(
        string text,
        CancellationToken ct = default);
}