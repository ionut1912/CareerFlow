using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;
using Microsoft.AspNetCore.Http;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IDocumentAnalyzerService
{
    Task<DocumentProcessingResponse> AnalyzeDocumentAsync(IFormFile document, CancellationToken ct);
    Task<ChapterDetailResponse> ExpandAnalyzedDocument(DocumentChapterRequest documentChapterRequest, CancellationToken ct);
}