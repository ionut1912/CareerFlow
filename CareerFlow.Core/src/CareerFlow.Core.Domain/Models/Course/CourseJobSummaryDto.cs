namespace CareerFlow.Core.Domain.Models.Course;

public record CourseJobSummaryDto(
    Guid JobId,
    string FileName,
    string Status);