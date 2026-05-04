namespace CareerFlow.Core.Domain.Constants;

public static class CourseConstants
{
    public const int MaxFiles = 15;
    public const long MaxFileSizeBytes = 20 * 1024 * 1024;

    public static readonly HashSet<string> AllowedExtensions =
    [
        ".pdf",
        ".doc",
        ".docx"
    ];
}
