using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record SubchapterSkeletonDto(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("content_summary")]
    string ContentSummary,
    [property: JsonPropertyName("estimated_minutes")]
    int EstimatedMinutes);