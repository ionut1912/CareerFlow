using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests.Course;
using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Application.Mappings;

public static class CourseMapping
{

    public static UploadCourseDocumentCommand ToUploadCourseDocumentCommand(this UploadCourseDocumentRequest request,
        Guid userId)
    {
        return new UploadCourseDocumentCommand(userId, request.Title, request.Files);
    }

    public static FinishChapterCommand ToFinishChapterCommand(this FinishChapterRequest request, Guid userId)
    {
        return new FinishChapterCommand(userId,request.CourseId, request.ChapterId);
    }

    public static GenerateCourseCommand ToGenerateCourseCommand(this CourseRequest request, Guid userId)
    {
        return new GenerateCourseCommand(userId, request.Topic);
    }

    private static CourseDto ToDto(this Course course)
    {
        return new CourseDto(course.Topic, course.Chapters.ToDto());
    }

    public static List<CourseDto> ToDto(this IEnumerable<Course> courses)
    {
        return courses.Select(c=>c.ToDto()).ToList();
    }
}