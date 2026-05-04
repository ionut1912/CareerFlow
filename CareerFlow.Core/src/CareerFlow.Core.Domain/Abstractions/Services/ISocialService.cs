namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ISocialService
{
    Task<string> GoogleMobileLogin(string? returnUrl = null);
    Task<string> GoogleMobileCallBackAsync(string code, string state, CancellationToken cancellationToken);

    Task<string> LinkedInMobileLogin(string? returnUrl = null);
    Task<string> LinkedInCallBackAsync(string code, string state, CancellationToken cancellationToken);
}
