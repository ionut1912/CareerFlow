using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record ChapterQuizResponse(
    [property: JsonPropertyName("chapter_title")]
    string ChapterTitle,
    [property: JsonPropertyName("quiz")] List<QuizQuestionDto> Quiz);