namespace CareerFlow.Core.Domain.Models.OpenAi;

public sealed record ChatRequest(
    string Model,
    List<ChatMessageDto> Messages,
    int MaxTokens,
    float Temperature
);
