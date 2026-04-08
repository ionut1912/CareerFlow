namespace CareerFlow.Core.Application.Responses;

public sealed record CourseJobStatusResponse(
    Guid JobId,
    string Status,
    Guid? CourseId,
    string? ErrorMessage);