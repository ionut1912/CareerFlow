using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record FullCourseResponse(
    [property: JsonPropertyName("analysis")]
    DocumentAnalysisResponse Analysis,
    [property: JsonPropertyName("skeleton")]
    LearningPlanSkeletonResponse Skeleton,
    [property: JsonPropertyName("chapters")]
    List<FullCourseChapterData> Chapters);