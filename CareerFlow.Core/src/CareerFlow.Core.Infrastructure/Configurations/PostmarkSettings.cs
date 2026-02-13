namespace CareerFlow.Core.Infrastructure.Configurations;

public class PostmarkSettings
{
    public static string SectionName => "PostmarkSettings";
    public string ServerToken { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
