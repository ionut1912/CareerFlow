using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.AI;

public sealed record DocumentAnalysisResponse(
    [property: JsonPropertyName("filename")]
    string FileName,
    [property: JsonPropertyName("total_pages")]
    int TotalPages,
    [property: JsonPropertyName("analysis")]
    DocumentAnalysis Analysis);