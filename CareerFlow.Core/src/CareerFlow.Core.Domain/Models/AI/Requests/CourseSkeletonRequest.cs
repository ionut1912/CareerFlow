using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI.Requests;


public sealed record CourseSkeletonRequest(
    [property: JsonPropertyName("topic")] string Topic);