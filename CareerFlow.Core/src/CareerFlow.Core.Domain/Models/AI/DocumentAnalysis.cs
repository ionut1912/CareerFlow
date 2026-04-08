using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record DocumentAnalysis(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("summary")]
    string Summary,
    [property: JsonPropertyName("key_topics")]
    List<string> KeyTopics,
    [property: JsonPropertyName("suggested_days")]
    int SuggestedDays);