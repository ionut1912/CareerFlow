using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record SubchapterContentResponse(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("theory_html")]
    string TheoryHtml,
    [property: JsonPropertyName("summary")]
    string Sumary,
    [property: JsonPropertyName("mini_quiz")]
    List<QuizQuestionDto> MiniQuiz);