using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI.Dto;

public sealed record SkeletonDto(
    [property: JsonPropertyName("topic")] string Topic,
    [property: JsonPropertyName("chapters")]
    List<ChapterDto> Chapters);