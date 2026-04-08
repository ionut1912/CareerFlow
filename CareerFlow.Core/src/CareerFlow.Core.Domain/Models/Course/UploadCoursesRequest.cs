using Microsoft.AspNetCore.Http;

namespace CareerFlow.Core.Application.Requests;

public sealed record UploadCoursesRequest
{
    public IFormFileCollection Files { get; init; } = null!;
    public string Title { get; init; } = string.Empty;
    public int NumberOfDays { get; init; }
}