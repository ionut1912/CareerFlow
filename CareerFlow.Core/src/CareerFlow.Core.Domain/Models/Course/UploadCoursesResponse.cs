using CareerFlow.Core.Application.Dtos;

namespace CareerFlow.Core.Application.Responses;

public sealed record UploadCoursesResponse(
    IReadOnlyList<CourseJobSummaryDto> Jobs,
    int TotalFiles,
    int Accepted,
    int Rejected,
    IReadOnlyList<string> Errors);