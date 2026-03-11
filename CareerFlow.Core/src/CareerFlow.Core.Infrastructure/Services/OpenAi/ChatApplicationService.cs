using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models.OpenAi;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Infrastructure.Services.OpenAi;

public sealed class ChatApplicationService
{
    private readonly IAICompletionService _completionService;
    private readonly ILogger<ChatApplicationService> _logger;

    public ChatApplicationService(
        IAICompletionService completionService,
        ILogger<ChatApplicationService> logger)
    {
        _completionService = completionService;
        _logger = logger;
    }

    public async Task<string> AskAsync(string userMessage, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);

        var request = new CompletionRequest(
            Prompt: userMessage,
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
            Prompt: userMessage,
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
