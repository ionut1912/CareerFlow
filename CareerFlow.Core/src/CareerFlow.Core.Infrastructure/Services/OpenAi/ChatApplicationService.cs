using CareerFlow.Core.Infrastructure.Modles.OpenAi;
using CareerFlow.Core.Infrastructure.OpenAIAbstractions;

namespace CareerFlow.Core.Infrastructure.Services.OpenAi;

public sealed class ChatApplicationService
{
    private readonly IAICompletionService _completionService;

    public ChatApplicationService(
        IAICompletionService completionService)
    {
        _completionService = completionService;
    }

    public async Task<string> AskAsync(string userMessage, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);

        var request = new CompletionRequest(
            userMessage,
            MaxTokens: 500,
            Temperature: 0.7f
        );

        var result = await _completionService.CompleteAsync(request, ct);
        return result.Content;
    }

    public async Task<string> AskWithSystemPromptAsync(
        string systemPrompt,
        string userMessage,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(systemPrompt);
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);

        var request = new CompletionRequest(
            userMessage,
            Messages:
            [
                new Message("system", systemPrompt),
                new Message("user", userMessage)
            ]
        );

        var result = await _completionService.CompleteAsync(request, ct);
        return result.Content;
    }
}