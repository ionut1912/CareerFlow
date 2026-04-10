using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI.Dto;

public sealed record QuestionOptionDto(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("is_correct")]
    bool IsCorrect);