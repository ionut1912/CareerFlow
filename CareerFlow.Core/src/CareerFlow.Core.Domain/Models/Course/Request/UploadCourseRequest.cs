using CareerFlow.Core.Domain.Models.Course.Dto;

namespace CareerFlow.Core.Domain.Models.Course.Request;

public sealed record UploadCoursesRequest
{
    public IEnumerable<UploadFileDto> Files { get; init; } = null!;
    public string Title { get; init; } = string.Empty;
    public int NumberOfDays { get; init; }
}