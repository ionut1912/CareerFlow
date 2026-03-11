namespace CareerFlow.Core.Domain.Models.OpenAi;

public sealed record ChatResponse(
    List<ChatChoiceDto> Choices,
    ChatUsageDto Usage
);
