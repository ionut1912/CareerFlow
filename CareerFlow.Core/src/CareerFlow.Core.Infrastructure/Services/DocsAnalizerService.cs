using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models.AI;
using Microsoft.AspNetCore.Http;

namespace CareerFlow.Core.Infrastructure.Services;

public class DocsAnalizerService : IDocumentAnalyzerService
{
    private static readonly JsonSerializerOptions SnakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public DocsAnalizerService(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromMinutes(10);
    }


    public async Task<FullCourseResponse> GenerateFullCourseAsync(IFormFile document, int numberOfDays,
        CancellationToken ct = default)
    {
        using var content = CreateMultipartContent(document);
        content.Add(new StringContent(numberOfDays.ToString()), "number_of_days");
        return await PostMultipartAsync<FullCourseResponse>("/document-courses/generate-full", content, ct);
    }

    private MultipartFormDataContent CreateMultipartContent(IFormFile file)
    {
        var content = new MultipartFormDataContent();
        var fileStream = file.OpenReadStream();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
        content.Add(streamContent, "file", file.FileName);
        return content;
    }

    private async Task<T> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content, CancellationToken ct)
    {
        var response = await _http.PostAsync(endpoint, content, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(SnakeCaseOptions, ct)
               ?? throw new InvalidOperationException($"Null response from {endpoint}");
    }
}