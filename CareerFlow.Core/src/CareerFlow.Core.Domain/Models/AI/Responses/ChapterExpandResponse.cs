using System.Text.Json.Serialization;

using CareerFlow.Core.Domain.Models.AI.Dto;

namespace CareerFlow.Core.Domain.Models.AI.Responses;

public sealed record ChapterExpandResponse(
    [property: JsonPropertyName("chapter")]
    ChapterDto Chapter,
    [property: JsonPropertyName("expanded")]
    ExpandedContentDto Expanded,
    [property: JsonPropertyName("subchapter_contents")]
    List<SubchapterContentDto> SubchapterContents,
    [property: JsonPropertyName("quiz")] List<QuestionDto> FinalQuiz);
