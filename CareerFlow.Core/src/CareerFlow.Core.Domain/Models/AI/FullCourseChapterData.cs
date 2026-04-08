using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record FullCourseChapterData(
    [property: JsonPropertyName("day")] int Day,
    [property: JsonPropertyName("chapter_title")]
    string ChapterTitle,
    [property: JsonPropertyName("core_concept")]
    string? CoreConcept,
    [property: JsonPropertyName("subchapters")]
    List<SubchapterContentResponse> Subchapters,
    [property: JsonPropertyName("quiz")] ChapterQuizResponse Quiz);