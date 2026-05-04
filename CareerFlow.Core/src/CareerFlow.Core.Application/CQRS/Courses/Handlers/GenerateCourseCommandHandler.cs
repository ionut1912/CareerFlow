using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models.AI.Requests;
using CareerFlow.Core.Domain.Models.AI.Responses;

namespace CareerFlow.Core.Application.CQRS.Courses.Handlers;

public class GenerateCourseCommandHandler
{
    private readonly ICourseService _courseService;

    public GenerateCourseCommandHandler(ICourseService courseService)
    {
        ArgumentNullException.ThrowIfNull(courseService);
        _courseService = courseService;
    }

    public async Task<Guid> Handle(GenerateCourseCommand request, CancellationToken cancellationToken)
    {
        var skeletonRequest = new CourseSkeletonRequest(request.Topic);
        CourseSkeletonResponse skeletonResponse =
            await _courseService.GetCourseSkeletonAsync(skeletonRequest, cancellationToken);
        return await _courseService.SaveCourseContentAsync(request.UserId, skeletonRequest.Topic, skeletonResponse,
            cancellationToken);
    }
}
