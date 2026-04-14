namespace CareerFlow.Core.Domain.Constants;

public static class CourseConstants
{
    public const int MaxFiles = 14;
    public const long MaxFileSizeBytes = 20 * 1024 * 1024;
    public const long MaxTotalBytes = MaxFiles * MaxFileSizeBytes;


    public static readonly HashSet<string> AllowedContentTypes =
    [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];

    public static readonly HashSet<string> AllowedExtensions =
    [
        ".pdf",
        ".doc",
        ".docx"
    ];
}