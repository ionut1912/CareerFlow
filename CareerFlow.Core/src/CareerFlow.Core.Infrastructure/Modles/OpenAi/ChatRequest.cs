namespace CareerFlow.Core.Infrastructure.Modles.OpenAi;

public sealed record ChatRequest(
    string Model,
    List<ChatMessageDto> Messages,
    int MaxTokens,
    float Temperature
);