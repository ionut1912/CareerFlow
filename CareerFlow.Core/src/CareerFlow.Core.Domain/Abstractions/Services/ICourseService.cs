using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;
using CareerFlow.Core.Domain.Models.Course;
using Microsoft.AspNetCore.Http;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ICourseService
{
    Task<UploadCoursesResponse> UploadManyAsync(
         Guid userId,IFormFileCollection files,string title, CancellationToken ct = default);

    Task FinishChapterAsync(
        Guid userId, Guid courseId, Guid chapterId, CancellationToken ct = default);
    
    Task<CourseSkeletonResponse> GetCourseSkeletonAsync(CourseSkeletonRequest request,CancellationToken ct = default);
    Task<Guid> SaveCourseContentAsync(Guid userId,string topic,CourseSkeletonResponse response, CancellationToken ct = default);
}