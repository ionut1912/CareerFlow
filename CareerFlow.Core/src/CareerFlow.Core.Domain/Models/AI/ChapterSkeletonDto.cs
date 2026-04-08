using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record ChapterSkeletonDto(
    [property: JsonPropertyName("day")] int Day,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("core_concept")]
    string CoreConcept);