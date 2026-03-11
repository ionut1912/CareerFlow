using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Infrastructure.Modles.OpenAi;
using CareerFlow.Core.Infrastructure.OpenAIAbstractions;

namespace CareerFlow.Core.Infrastructure.Services.OpenAi;

public sealed class OpenAIEmbeddingService : IAIEmbeddingService
{
    private readonly IOpenAIHttpClient _client;

    public OpenAIEmbeddingService(
        IOpenAIHttpClient client)
    {
        _client = client;
    }

    public async Task<EmbeddingResult> GetEmbeddingAsync(
        string text,
        CancellationToken ct = default)
    {
        var payload = new EmbeddingRequest(
            text,
            "text-embedding-3-small"
        );

        var response = await _client.CreateAsync<EmbeddingRequest, EmbeddingResponse>(
            "embeddings", payload, ct);

        return MapToResult(response);
    }

    private static EmbeddingResult MapToResult(EmbeddingResponse response)
    {
        return new EmbeddingResult(
            response.Data[0].Embedding,
            response.Usage.TotalTokens
        );
    }
}