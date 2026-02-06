using CareerFlow.Core.Domain.Entities;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Domain.Interfaces;

public interface IRefreshTokenService:IGenericRepository<RefreshToken>
{
    Task<RefreshToken> GetExistingTokenAsync(string token,CancellationToken cancellationToken);
}
