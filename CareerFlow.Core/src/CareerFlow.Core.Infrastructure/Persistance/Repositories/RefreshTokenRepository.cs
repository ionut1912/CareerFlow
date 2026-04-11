using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistance.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    private readonly DbSet<RefreshToken> _refreshTokens;

    public RefreshTokenRepository(DbSet<RefreshToken> dbSet) : base(dbSet)
    {
        _refreshTokens = dbSet;
    }

    public async Task<RefreshToken?> GetExistingTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        return await _refreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == refreshToken, cancellationToken);
    }
}