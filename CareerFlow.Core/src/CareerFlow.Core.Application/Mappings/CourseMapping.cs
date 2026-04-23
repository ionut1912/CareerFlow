using CareerFlow.Core.Application.CQRS.Courses.Commands;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests.Course;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.Course.Dto;

namespace CareerFlow.Core.Application.Mappings;

public static class CourseMapping
{
    public static UploadCourseDocumentCommand ToUploadCourseDocumentCommand(this UploadCourseDocumentRequest request,
        Guid userId)
    {
        var files = request.Files.Select(f => new UploadFileDto(
                f.FileName,
                f.ContentType,
                f.OpenReadStream()))
            .ToList();
        return new UploadCourseDocumentCommand(userId, request.Title, files);
    }

    public static FinishChapterCommand ToFinishChapterCommand(this FinishChapterRequest request, Guid userId) =>
        new(userId, request.CourseId, request.ChapterId);

    public static GenerateCourseCommand ToGenerateCourseCommand(this CourseRequest request, Guid userId) =>
        new(userId, request.Topic);

    private static CourseDto ToDto(this Course course) => new(course.Topic, course.Chapters.ToDto());

    public static List<CourseDto> ToDto(this IEnumerable<Course> courses) => courses.Select(c => c.ToDto()).ToList();
}
