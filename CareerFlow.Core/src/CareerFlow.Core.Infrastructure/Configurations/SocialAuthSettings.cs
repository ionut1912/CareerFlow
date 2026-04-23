namespace CareerFlow.Core.Infrastructure.Configurations;

public class SocialAuthSettings
{
    public static string SectionName => "Authentication";
    public string BaseUrl { get; init; } = string.Empty;
    public required GoogleSettings Google { get; init; }
    public required LinkedInSettings LinkedIn { get; init; }
}
