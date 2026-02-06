using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class TermsAndCondition : Entity
{

    public string Content { get; private set; }= string.Empty;

    private TermsAndCondition()
    {
    }

    public TermsAndCondition(string content)
    {
        Content = content;
    }

    public static TermsAndCondition CreateContent(string content)
    {
        return new TermsAndCondition(content);
    }

    public void UpdateContent(string content)
    {
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }
}
