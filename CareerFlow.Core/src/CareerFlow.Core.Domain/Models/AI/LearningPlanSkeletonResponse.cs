using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record LearningPlanSkeletonResponse(
    [property: JsonPropertyName("topic")] string Topic,
    [property: JsonPropertyName("chapters")]
    List<ChapterSkeletonDto> Chapters);