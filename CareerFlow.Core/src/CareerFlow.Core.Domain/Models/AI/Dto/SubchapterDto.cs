using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI.Dto;

public sealed record SubchapterDto(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("content_summary")]
    string ContentSummary);
