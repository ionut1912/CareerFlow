using CareerFlow.Core.Domain.Exceptions;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class SystemDocument : Entity
{
    private SystemDocument()
    {
    }

    private SystemDocument(string documentType, string currentETag)
    {
        if (string.IsNullOrWhiteSpace(documentType))
            throw new InvalidFieldException("Tipul documentului este necesar");
        if (string.IsNullOrWhiteSpace(currentETag))
            throw new InvalidFieldException("Etag-ul documentului este necesar");
        DocumentType = documentType;
        CurrentETag = currentETag;
        CreatedAt = DateTime.UtcNow;
    }

    public string DocumentType { get; private set; } = string.Empty;
    public string CurrentETag { get; private set; } = string.Empty;

    public static SystemDocument Create(string documentType, string currentETag)
    {
        return new SystemDocument(documentType, currentETag);
    }

    public void Update(string currentEtag)
    {
        if (string.IsNullOrWhiteSpace(currentEtag))
            throw new InvalidFieldException("Etag-ul documentului este necesar");

        if (currentEtag == CurrentETag)
            throw new DocumentEtagExistsException("Etag-ul documentului nu poate fi acelasi");

        CurrentETag = currentEtag;
    }
}