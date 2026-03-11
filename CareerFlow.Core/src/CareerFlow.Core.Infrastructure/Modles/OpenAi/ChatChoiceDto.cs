namespace CareerFlow.Core.Infrastructure.Modles.OpenAi;

public sealed record ChatChoiceDto(
    ChatMessageDto Message,
    string FinishReason
);