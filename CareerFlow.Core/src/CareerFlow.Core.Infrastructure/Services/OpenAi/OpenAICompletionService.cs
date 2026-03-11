using CareerFlow.Core.Domain.Abstractions.Http;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models.OpenAi;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace CareerFlow.Core.Infrastructure.Services.OpenAi;

public sealed class OpenAICompletionService : IAICompletionService
{
    private readonly IOpenAIHttpClient _client;
    private readonly OpenAIOptions _options;

    public OpenAICompletionService(
        IOpenAIHttpClient client,
        IOptions<OpenAIOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<CompletionResult> CompleteAsync(
        CompletionRequest request,
        CancellationToken ct = default)
    {
        var payload = BuildPayload(request);

        var response = await _client.PostAsync<ChatRequest, ChatResponse>(
            "chat/completions", payload, ct);

        return MapToResult(response);
    }

    private ChatRequest BuildPayload(CompletionRequest request) => new(
        Model: request.Model ?? _options.DefaultModel,
        Messages: request.Messages?
                      .Select(m => new ChatMessageDto(m.Role, m.Content))
                      .ToList()
                  ?? [new ChatMessageDto("user", request.Prompt)],
        MaxTokens: request.MaxTokens,
        Temperature: request.Temperature
    );

    private static CompletionResult MapToResult(ChatResponse response) => new(
        Content: response.Choices[0].Message.Content,
        TokensUsed: response.Usage.TotalTokens,
        FinishReason: response.Choices[0].FinishReason
    );
}