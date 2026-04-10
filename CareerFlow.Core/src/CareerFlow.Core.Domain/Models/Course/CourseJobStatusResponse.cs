namespace CareerFlow.Core.Domain.Models.Course;

public sealed record CourseJobStatusResponse(
    Guid JobId,
    string Status,
    Guid? CourseId,
    string? ErrorMessage);