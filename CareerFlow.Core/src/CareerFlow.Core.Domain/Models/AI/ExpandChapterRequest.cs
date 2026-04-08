using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record ExpandChapterRequest(
    [property: JsonPropertyName("topic")] string Topic,
    [property: JsonPropertyName("chapter")]
    ChapterSkeletonDto Chapter);