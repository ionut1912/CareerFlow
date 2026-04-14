using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Domain.Abstractions.Services;

namespace CareerFlow.Core.Application.CQRS.Courses.Handlers;

public class FinishChapterCommandHandler
{
    private readonly ICourseService _courseService;

    public FinishChapterCommandHandler(ICourseService courseService)
    {
        ArgumentNullException.ThrowIfNull(courseService);
        _courseService = courseService;
    }

    public async Task Handle(FinishChapterCommand request, CancellationToken cancellationToken)
    {
        await _courseService.FinishChapterAsync(request.UserId, request.CourseId, request.ChapterId, cancellationToken);
    }
}