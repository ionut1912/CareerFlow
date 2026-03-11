namespace CareerFlow.Core.Infrastructure.Modles.OpenAi;

public sealed record ChatResponse(
    List<ChatChoiceDto> Choices,
    ChatUsageDto Usage
);