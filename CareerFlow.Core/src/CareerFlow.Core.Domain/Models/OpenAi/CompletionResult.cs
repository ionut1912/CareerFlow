namespace CareerFlow.Core.Domain.Models.OpenAi;

public record CompletionResult(
    string Content,
    int TokensUsed,
    string FinishReason
);