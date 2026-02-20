using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class LegalDoc : Entity
{
    private LegalDoc()
    {
        Content = null!;
        Type = null!;
    }

    public LegalDoc(string contnet, string type)
    {
        if (string.IsNullOrWhiteSpace(contnet))
            throw new InvalidFieldException("Continutul este invalid");
        if (string.IsNullOrWhiteSpace(type))
            throw new InvalidFieldException("Tipul este invalid");

        Content = contnet;
        Type = LegalDocType.FromString(type);
        CreatedAt = DateTime.UtcNow;
    }

    public string Content { get; private set; }
    public LegalDocType Type { get; private set; }

    public static LegalDoc Create(string content, string type)
    {
        return new LegalDoc(content, type);
    }

    public void Update(string content, string type)
    {
        Content = content;
        Type = LegalDocType.FromString(type);
        UpdatedAt = DateTime.UtcNow;
    }
}