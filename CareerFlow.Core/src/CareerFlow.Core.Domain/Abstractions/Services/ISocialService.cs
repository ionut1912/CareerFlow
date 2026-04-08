namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ISocialService
{
    string GoogleMobileLogin(string? returnUrl = null);
    Task<string> GoogleMobileCallBack(string code, string state, CancellationToken cancellationToken);

    string LinkedInMobileLogin(string? returnUrl = null);
    Task<string> LinkedInCallBack(string code, string state, CancellationToken cancellationToken);
}