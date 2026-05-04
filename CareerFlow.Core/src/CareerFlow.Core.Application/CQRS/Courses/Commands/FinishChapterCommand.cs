namespace CareerFlow.Core.Application.CQRS.Courses.Commands;

public sealed record FinishChapterCommand(Guid UserId, Guid CourseId, Guid ChapterId);
