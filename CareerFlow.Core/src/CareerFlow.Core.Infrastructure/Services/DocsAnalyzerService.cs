using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;
using CareerFlow.Core.Domain.Models.Course.Dto;

namespace CareerFlow.Core.Infrastructure.Services;

public class DocsAnalyzerService : IDocumentAnalyzerService
{
    private static readonly JsonSerializerOptions _snakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public DocsAnalyzerService(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromMinutes(10);
    }

    public async Task<DocumentProcessingResponse> AnalyzeDocumentAsync(UploadFileDto document, CancellationToken ct)
    {
        using MultipartFormDataContent content = CreateMultipartContent(document);
        return await PostAsync<DocumentProcessingResponse>("/document-courses/upload-and-analyze", content, ct);
    }

    public async Task<ChapterDetailResponse> ExpandAnalyzedDocument(DocumentChapterRequest documentChapterRequest,
        CancellationToken ct)
    {
        HttpResponseMessage response = await _http.PostAsJsonAsync("/document-courses/chapters/expand", documentChapterRequest, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ChapterDetailResponse>(_snakeCaseOptions, ct)
               ?? throw new InvalidOperationException("Null response from endpoint");
    }

    private static MultipartFormDataContent CreateMultipartContent(UploadFileDto file)
    {
        var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(file.Content);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "file", file.FileName);
        return content;
    }

    private async Task<T> PostAsync<T>(string endpoint, MultipartFormDataContent content, CancellationToken ct)
    {
        HttpResponseMessage response = await _http.PostAsync(endpoint, content, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(_snakeCaseOptions, ct)
               ?? throw new InvalidOperationException($"Null response from {endpoint}");
    }
}
