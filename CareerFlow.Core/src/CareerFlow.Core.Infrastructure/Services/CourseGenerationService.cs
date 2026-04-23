using System.Net.Http.Json;
using System.Text.Json;

using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;

namespace CareerFlow.Core.Infrastructure.Services;

public class CourseGenerationService : IAnalyzerService
{
    private static readonly JsonSerializerOptions _snakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public CourseGenerationService(HttpClient http)
    {
        _http = http;
        _http.Timeout = TimeSpan.FromMinutes(10);
    }

    public async Task<CourseSkeletonResponse> GetCourseSkeletonAsync(CourseSkeletonRequest request,
        CancellationToken ct)
    {
        HttpResponseMessage response = await _http.PostAsJsonAsync("/courses/skeleton", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CourseSkeletonResponse>(_snakeCaseOptions, ct) ??
               throw new InvalidOperationException("Null response from endpoint");
    }

    public async Task<ChapterExpandResponse> GetExpandedChapterAsync(ChapterRequest request, CancellationToken ct)
    {
        HttpResponseMessage response = await _http.PostAsJsonAsync("/courses//chapters/expand", request, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ChapterExpandResponse>(_snakeCaseOptions, ct) ??
               throw new InvalidOperationException("Null response from endpoint");
    }
}
