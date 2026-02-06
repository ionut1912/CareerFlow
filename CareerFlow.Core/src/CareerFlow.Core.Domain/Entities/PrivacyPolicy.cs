using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.Entities;

public class PrivacyPolicy : Entity
{
    public string Content { get; private set; } = string.Empty;

    private PrivacyPolicy()
    {
        
    }

    public PrivacyPolicy(string content)
    {
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    public static PrivacyPolicy CreateContent(string content)
    {
        return new PrivacyPolicy(content);
    }

    public void UpdateContent(string content)
    {
        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }

}
