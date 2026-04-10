namespace CareerFlow.Core.Domain.Models.Course;

public sealed record UploadCoursesResponse(
    IReadOnlyList<CourseJobSummaryDto> Jobs,
    int TotalFiles,
    int Accepted,
    int Rejected,
    IReadOnlyList<string> Errors);