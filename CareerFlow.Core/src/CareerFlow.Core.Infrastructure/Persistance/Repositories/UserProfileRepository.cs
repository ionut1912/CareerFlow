using CareerFlow.Core.Domain.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistance.Repositories;

public class UserProfileRepository(DbSet<UserProfile> dbSet)
    : GenericRepository<UserProfile>(dbSet), IUserProfileRepository
{
    public async Task<UserProfile?> GetCurrentUserProfile(Guid accountId, CancellationToken cancellationToken)
    {
        return await dbSet
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.AccountId == accountId, cancellationToken);
    }
}