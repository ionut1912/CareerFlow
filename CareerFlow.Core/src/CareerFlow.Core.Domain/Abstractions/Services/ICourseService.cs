using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.Course.Dto;
using CareerFlow.Core.Domain.Models.Course.Response;
using CareerFlow.Core.Domain.Models.Responses;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ICourseService
{
    Task<UploadCoursesResponse> UploadManyAsync(
        Guid userId, IEnumerable<UploadFileDto> files, string title, CancellationToken ct = default);

    Task FinishChapterAsync(
        Guid userId, Guid courseId, Guid chapterId, CancellationToken ct = default);

    Task<CourseSkeletonResponse> GetCourseSkeletonAsync(
        CourseSkeletonRequest request, CancellationToken ct = default);

    Task<Guid> SaveCourseContentAsync(
        Guid userId, string topic, CourseSkeletonResponse response, CancellationToken ct = default);
}