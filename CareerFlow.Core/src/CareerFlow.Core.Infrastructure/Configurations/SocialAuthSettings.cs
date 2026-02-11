namespace CareerFlow.Core.Infrastructure.Configurations;

public class SocialAuthSettings
{
    public GoogleSettings Google { get; set; } = new();
    public LinkedInSettings LinkedIn { get; set; } = new();
}
