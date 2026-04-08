using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record CourseSkeletonRequest(
    [property: JsonPropertyName("topic")] string Topic,
    [property: JsonPropertyName("number_of_days")]
    int NumberOfDays);