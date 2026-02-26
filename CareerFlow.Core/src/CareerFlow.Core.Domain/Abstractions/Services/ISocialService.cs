namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ISocialService
{
    string GoogleMobileLogin();
    Task<string> GoogleMobileCallBack(string code, CancellationToken cancellationToken);
    string LinkedInMobileLogin();
    Task<string> LinkedInCallBack(string code, CancellationToken cancellationToken);
}