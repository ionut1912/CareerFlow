using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository(DbSet<RefreshToken> dbSet)
    : GenericRepository<RefreshToken>(dbSet), IRefreshTokenRepository
{
    private readonly DbSet<RefreshToken> _refreshTokens = dbSet;

    public async Task<RefreshToken?> GetExistingTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await _refreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == refreshToken, cancellationToken);
    }
}
