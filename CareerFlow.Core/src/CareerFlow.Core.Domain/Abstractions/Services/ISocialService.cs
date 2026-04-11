namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ISocialService
{
    string GoogleMobileLogin(string? returnUrl = null);
    Task<string> GoogleMobileCallBackAsync(string code, string state, CancellationToken cancellationToken);

    string LinkedInMobileLogin(string? returnUrl = null);
    Task<string> LinkedInCallBackAsync(string code, string state, CancellationToken cancellationToken);
}