namespace CareerFlow.Core.Domain.Models.OpenAi;

public record CompletionRequest(
    string Prompt,
    string? Model = null,
    int MaxTokens = 1000,
    float Temperature = 0.7f,
    IReadOnlyList<Message>? Messages = null
);