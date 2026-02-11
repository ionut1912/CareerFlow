using CareerFlow.Core.Domain.ValueObjects;
using Shared.Domain.Common;


namespace CareerFlow.Core.Domain.Entities;

public class LegalDoc : Entity
{
    public string Content { get; private set; } = string.Empty;
    public LegalDocType Type { get; private set; } = LegalDocType.TermsOfService;

    private LegalDoc()
    {

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
