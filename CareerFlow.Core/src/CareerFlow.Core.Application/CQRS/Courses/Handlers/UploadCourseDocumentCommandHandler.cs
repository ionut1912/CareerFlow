using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models.Course;
using CareerFlow.Core.Domain.Models.Course.Response;

namespace CareerFlow.Core.Application.CQRS.Courses.Handlers;

public class UploadCourseDocumentCommandHandler
{
    private readonly ICourseService _courseService;

    public UploadCourseDocumentCommandHandler(ICourseService courseService)
    {
        ArgumentNullException.ThrowIfNull(courseService, nameof(courseService));
        _courseService = courseService;
    }

    public async Task<UploadCoursesResponse> Handle(UploadCourseDocumentCommand request, CancellationToken cancellationToken)
    {
      return await  _courseService.UploadManyAsync(request.UserId, request.Files, request.Title,cancellationToken);
    }
}