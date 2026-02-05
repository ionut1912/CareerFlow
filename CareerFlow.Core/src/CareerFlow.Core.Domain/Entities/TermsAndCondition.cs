using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class TermsAndCondition : Entity
{

    public string Content { get; private set; }
    public bool Accepted { get; private set; } = false;


    public TermsAndCondition(string content)
    {
        Content = content;
    }

    public static TermsAndCondition CreateContent(string content)
    {
        return new TermsAndCondition(content);
    }

    public void Accept()
    {
        Accepted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContent(string content)
    {
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }   
}
