namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ISocialService
{
    string GoogleMobileLogin();
    Task<string> GoogleMobileCallBack(string code, string state, CancellationToken cancellationToken);
    string LinkedInMobileLogin();
    Task<string> LinkedInCallBack(string code, string state, CancellationToken cancellationToken);
}