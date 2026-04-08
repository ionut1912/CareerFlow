using CareerFlow.Core.Application.Requests;
using CareerFlow.Core.Application.Responses;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ICourseService
{
    Task<UploadCoursesResponse> UploadManyAsync(
        Guid userId, UploadCoursesRequest request, CancellationToken ct = default);

    Task<IEnumerable<CourseJobStatusResponse>> GetJobStatusesAsync(
        Guid[] jobIds, CancellationToken ct = default);

    Task FinishChapterAsync(
        Guid userId, Guid courseId, Guid chapterId, CancellationToken ct = default);
}