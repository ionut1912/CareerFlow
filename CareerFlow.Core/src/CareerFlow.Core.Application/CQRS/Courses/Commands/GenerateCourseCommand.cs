namespace CareerFlow.Core.Application.CQRS.Courses.Commands;

public record GenerateCourseCommand(Guid UserId,string Topic);