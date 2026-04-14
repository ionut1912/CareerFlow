using System.Text.Json.Serialization;
using CareerFlow.Core.Domain.Models.AI.Dto;

namespace CareerFlow.Core.Domain.Models.Responses;

public sealed record CourseSkeletonResponse(
    [property: JsonPropertyName("skeleton")]
    SkeletonDto Skeleton,
    [property: JsonPropertyName("estimated_days")]
    int EstimatedDays);