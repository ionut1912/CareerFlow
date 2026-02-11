using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistance.Repositories;

public class RefreshTokenRepository(DbSet<RefreshToken> dbSet) : GenericRepository<RefreshToken>(dbSet), IRefreshTokenRepository
{


    public async Task<RefreshToken?> GetExistingTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await dbSet.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }
}
