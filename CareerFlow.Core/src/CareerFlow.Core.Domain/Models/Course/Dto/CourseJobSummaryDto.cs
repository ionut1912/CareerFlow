namespace CareerFlow.Core.Domain.Models.Course.Dto;

public record CourseJobSummaryDto(
    Guid JobId,
    string FileName,
    string Status);