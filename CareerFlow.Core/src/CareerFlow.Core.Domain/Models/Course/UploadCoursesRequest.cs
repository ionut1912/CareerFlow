using Microsoft.AspNetCore.Http;

namespace CareerFlow.Core.Domain.Models.Course;

public sealed record ploadCoursesRequest
{
    public IFormFileCollection Files { get; init; } = null!;
    public string Title { get; init; } = string.Empty;
    public int NumberOfDays { get; init; }
}