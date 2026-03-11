namespace CareerFlow.Core.Infrastructure.Modles.OpenAi;

public record CompletionResult(
    string Content,
    int TokensUsed,
    string FinishReason
);