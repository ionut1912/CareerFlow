
using CareerFlow.Core.Domain.Models.Course.Dto;

namespace CareerFlow.Core.Domain.Models.Course.Response;

public sealed record UploadCoursesResponse(
    List<CourseJobSummaryDto> Jobs,
    int TotalFiles,
    int Accepted,
    int Rejected,
    IReadOnlyList<string> Errors);