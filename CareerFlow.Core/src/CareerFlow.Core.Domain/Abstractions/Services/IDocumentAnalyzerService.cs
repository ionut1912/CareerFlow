using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;
using CareerFlow.Core.Domain.Models.Course.Dto;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IDocumentAnalyzerService
{
    Task<DocumentProcessingResponse> AnalyzeDocumentAsync(UploadFileDto document, CancellationToken ct);

    Task<ChapterDetailResponse> ExpandAnalyzedDocument(DocumentChapterRequest documentChapterRequest,
        CancellationToken ct);
}