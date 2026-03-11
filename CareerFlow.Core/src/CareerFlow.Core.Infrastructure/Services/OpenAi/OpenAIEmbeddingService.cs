using CareerFlow.Core.Domain.Abstractions.Http;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models.OpenAi;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace CareerFlow.Core.Infrastructure.Services.OpenAi;

public sealed class OpenAIEmbeddingService : IAIEmbeddingService
{
    private readonly IOpenAIHttpClient _client;
    private readonly OpenAIOptions _options;

    public OpenAIEmbeddingService(
        IOpenAIHttpClient client,
        IOptions<OpenAIOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<EmbeddingResult> GetEmbeddingAsync(
        string text,
        CancellationToken ct = default)
    {
        var payload = new EmbeddingRequest(
            Input: text,
            Model: "text-embedding-3-small"
        );

        var response = await _client.PostAsync<EmbeddingRequest, EmbeddingResponse>(
            "embeddings", payload, ct);

        return MapToResult(response);
    }

    private static EmbeddingResult MapToResult(EmbeddingResponse response) => new(
        Vector: response.Data[0].Embedding,
        TokensUsed: response.Usage.TotalTokens
    );
}