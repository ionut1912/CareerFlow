using CareerFlow.Core.Domain.Exceptions;

using JetBrains.Annotations;

using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public sealed class CourseUpload : Entity
{
    [UsedImplicitly]
    private CourseUpload() //For EfCore
    {
    }

    private CourseUpload(Guid userId, string title, string fileName, string fileKey, string fileType)
    {
        if (userId == Guid.Empty) throw new InvalidFieldException("Id-ul utilizatorului este necesar");

        if (string.IsNullOrWhiteSpace(title)) throw new InvalidFieldException("Titlul este necesar");

        if (string.IsNullOrWhiteSpace(fileName)) throw new InvalidFieldException("Numele fisierului este necesar");

        if (string.IsNullOrWhiteSpace(fileKey)) throw new InvalidFieldException("Cheia fisierului este necesara");

        if (string.IsNullOrWhiteSpace(fileType)) throw new InvalidFieldException("Typ este necesar");
        UserId = userId;
        Title = title;
        FileName = fileName;
        FileKey = fileKey;
        FileType = fileType;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string FileKey { get; private set; } = string.Empty;
    public string FileType { get; private set; } = string.Empty;

    [UsedImplicitly] public CourseJob? Job { get; private set; }

    public static CourseUpload Create(Guid userId, string title, string fileName, string fileKey, string fileType) =>
        new(userId, title, fileName, fileKey, fileType);
}
