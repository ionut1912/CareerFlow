using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI.Dto;

public sealed record QuestionDto(
    [property: JsonPropertyName("question")]
    string Question,
    [property: JsonPropertyName("options")]
    List<QuestionOptionDto> Options);
