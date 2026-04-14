using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI.Dto;

public sealed record ChapterDto(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("core_concept")]
    string CoreConcept,
    [property: JsonPropertyName("day")] int Day);