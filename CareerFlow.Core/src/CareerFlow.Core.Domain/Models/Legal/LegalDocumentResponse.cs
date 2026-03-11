using System.Text.Json.Serialization;

namespace CareerFlow.Core.Domain.Models.Legal;

[JsonSerializable(typeof(LegalDocumentResponse))]
public record LegalDocumentResponse(string Content, string Source, DateTime LastChecked);