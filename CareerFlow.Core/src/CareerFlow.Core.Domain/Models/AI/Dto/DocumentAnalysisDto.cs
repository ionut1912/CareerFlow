using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI.Dto;

public sealed record DocumentAnalysisDto(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("summary")]
    string Summary,
    [property: JsonPropertyName("key_topics")]
    List<string> KeyTopics);