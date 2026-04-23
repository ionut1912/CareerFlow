using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IAnalyzerService
{
    Task<CourseSkeletonResponse> GetCourseSkeletonAsync(CourseSkeletonRequest request, CancellationToken ct);
    Task<ChapterExpandResponse> GetExpandedChapterAsync(ChapterRequest request, CancellationToken ct);
}
