using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI.Dto;

public sealed record SubchapterContentsDto(
    [property: JsonPropertyName("subchapter_contents")]
    List<SubchapterContentDto> SubchapterContents);