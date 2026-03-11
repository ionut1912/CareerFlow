namespace CareerFlow.Core.Domain.Models.OpenAi;

public sealed record ChatChoiceDto(
    ChatMessageDto Message,
    string FinishReason
);