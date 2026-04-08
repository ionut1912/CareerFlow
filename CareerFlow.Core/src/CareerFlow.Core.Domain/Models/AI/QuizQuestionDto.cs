using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record QuizQuestionDto(
    [property: JsonPropertyName("question")]
    string Question,
    [property: JsonPropertyName("options")]
    List<string> Options,
    [property: JsonPropertyName("correct_answer")]
    string CorrectAnswer);