namespace CareerFlow.Core.Infrastructure.Modles.OpenAi;

public record CompletionRequest(
    string Prompt,
    string? Model = null,
    int MaxTokens = 1000,
    float Temperature = 0.7f,
    IReadOnlyList<Message>? Messages = null
);