using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record ExpandedDayResponse(
    [property: JsonPropertyName("day")] int Day,
    [property: JsonPropertyName("chapter_title")]
    string ChapterTitle,
    [property: JsonPropertyName("subchapters")]
    List<SubchapterSkeletonDto> Subchapters);