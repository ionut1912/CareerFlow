using CareerFlow.Core.Domain.Entities;

namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IAuthService
{
    Task<Account> LoginWithGoogleAsync(string idToken, CancellationToken ct = default);
    Task<Account> LoginWithLinkedInAsync(string authorizationCode, CancellationToken ct = default);
}
