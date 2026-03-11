using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Modles.OpenAi;
using CareerFlow.Core.Infrastructure.OpenAIAbstractions;
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

        var response = await _client.CreateAsync<ChatRequest, ChatResponse>(
            "chat/completions", payload, ct);

        return MapToResult(response);
    }

    private ChatRequest BuildPayload(CompletionRequest request)
    {
        return new ChatRequest(
            request.Model ?? _options.DefaultModel,
            request.Messages?
                .Select(m => new ChatMessageDto(m.Role, m.Content))
                .ToList()
            ?? [new ChatMessageDto("user", request.Prompt)],
            request.MaxTokens,
            request.Temperature
        );
    }

    private static CompletionResult MapToResult(ChatResponse response)
    {
        var choice = response.Choices?.FirstOrDefault() ??
                     throw new OpenAIException(0, "No choices returned from OpenAI");
        return new CompletionResult(
            choice.Message?.Content ?? string.Empty,
            response.Usage?.TotalTokens ?? 0,
            choice.FinishReason ?? "unknown");
    }
}