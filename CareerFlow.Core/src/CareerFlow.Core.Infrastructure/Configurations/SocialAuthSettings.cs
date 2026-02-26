namespace CareerFlow.Core.Infrastructure.Configurations;

public class SocialAuthSettings
{
    public static string SectionName => "Authentication";
    public string BaseUrl { get; set; } = string.Empty;
    public GoogleSettings Google { get; set; } = new();
    public LinkedInSettings LinkedIn { get; set; } = new();
}