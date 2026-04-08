using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record SubchapterContentRequest(
    [property: JsonPropertyName("topic")] string Topic,
    [property: JsonPropertyName("chapter_title")]
    string ChapterTitle,
    [property: JsonPropertyName("subchapter")]
    SubchapterSkeletonDto Subchapter);