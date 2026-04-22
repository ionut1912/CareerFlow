using CareerFlow.Core.Domain.Models.Course.Dto;
using JetBrains.Annotations;

namespace CareerFlow.Core.Domain.Models.Course.Response;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed record UploadCoursesResponse(
    List<CourseJobSummaryDto> Jobs,
    int TotalFiles,
    int Accepted,
    int Rejected,
    IReadOnlyList<string> Errors);