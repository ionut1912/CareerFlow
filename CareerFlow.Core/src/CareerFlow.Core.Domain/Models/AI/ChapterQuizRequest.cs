using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record ChapterQuizRequest(
    [property: JsonPropertyName("topic")] string Topic,
    [property: JsonPropertyName("chapter_title")]
    string ChapterTitle,
    [property: JsonPropertyName("subchapters")]
    List<SubchapterSkeletonDto> Subchapters);