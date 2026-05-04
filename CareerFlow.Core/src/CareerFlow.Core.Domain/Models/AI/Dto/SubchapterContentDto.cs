using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI.Dto;

public sealed record SubchapterContentDto(
    [property: JsonPropertyName("theory_html")]
    string TheoryHtml,
    [property: JsonPropertyName("quiz")] List<QuestionDto> Quiz);
