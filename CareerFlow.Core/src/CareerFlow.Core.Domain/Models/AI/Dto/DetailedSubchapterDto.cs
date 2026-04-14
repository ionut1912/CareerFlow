using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI.Dto;

public sealed record DetailedSubchapterDto(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("content_summary")]
    string ContentSummary,
    [property: JsonPropertyName("theory_html")]
    string TheoryHtml,
    [property: JsonPropertyName("quiz")] List<QuestionDto> Quiz);