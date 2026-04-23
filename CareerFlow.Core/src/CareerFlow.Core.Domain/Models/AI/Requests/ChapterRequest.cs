using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace CareerFlow.Core.Domain.Models.AI.Requests;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record ChapterRequest(
    [property: JsonPropertyName("topic")] string Topic,
    [property: JsonPropertyName("chapter_title")]
    string ChapterTitle,
    [property: JsonPropertyName("core_concept")]
    string CoreConcept);
