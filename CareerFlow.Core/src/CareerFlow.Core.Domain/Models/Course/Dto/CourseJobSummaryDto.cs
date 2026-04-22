using JetBrains.Annotations;

namespace CareerFlow.Core.Domain.Models.Course.Dto;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record CourseJobSummaryDto(
    Guid JobId,
    string FileName,
    string Status);