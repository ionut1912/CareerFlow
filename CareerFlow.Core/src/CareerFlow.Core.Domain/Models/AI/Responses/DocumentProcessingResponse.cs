using System.Text.Json.Serialization;
using CareerFlow.Core.Domain.Models.AI.Dto;

namespace CareerFlow.Core.Domain.Models.AI.Responses;

public sealed record DocumentProcessingResponse(
    [property: JsonPropertyName("document_id")] string DocumentId,
    [property: JsonPropertyName("analysis")] DocumentAnalysisDto Analysis,
    [property: JsonPropertyName("skeleton")] SkeletonDto Skeleton,
    [property: JsonPropertyName("estimated_days")] int EstimatedDays);