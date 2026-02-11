using CareerFlow.Core.Domain.ValueObjects;
using Shared.Domain.Common;


namespace CareerFlow.Core.Domain.Entities;

public class LegalDoc : Entity
{
    public string Content { get; private set; }
    public LegalDocType Type { get; private set; }

    private LegalDoc()
    {
        Content = null!;
        Type = null!;
    }

    public LegalDoc(string contnet, string type)
    {
        Content = contnet;
        Type = LegalDocType.FromString(type);
        CreatedAt = DateTime.UtcNow;
    }

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
