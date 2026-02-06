using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CarrerFlow.Core.Infrastructure.Persistance.Repositories;

public class RefreshTokenService(DbSet<RefreshToken> dbSet) :GenericRepository<RefreshToken>(dbSet), IRefreshTokenService
{


    public async Task<RefreshToken> GetExistingTokenAsync(string token, CancellationToken cancellationToken)
    {
       return await dbSet.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }
}
