namespace CareerFlow.Core.Application.Requests.Course;

public sealed record FinishChapterRequest(Guid CourseId, Guid ChapterId);
