using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI.Dto;

public sealed record ExpandedContentDto(
    [property: JsonPropertyName("subchapters")]
    List<SubchapterDto> Subchapters);