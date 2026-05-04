namespace CareerFlow.Core.Infrastructure.Configurations;

public class LinkedInSettings
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RedirectUri { get; init; }
}
