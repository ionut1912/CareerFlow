using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistance.Repositories;

public class UserProfileRepository : GenericRepository<UserProfile>, IUserProfileRepository
{
    private readonly DbSet<UserProfile> _userProfiles;

    public UserProfileRepository(DbSet<UserProfile> dbSet) : base(dbSet)
    {
        _userProfiles = dbSet;
    }

    public async Task<UserProfile?> GetCurrentUserProfile(Guid accountId, CancellationToken cancellationToken)
    {
<<<<<<< HEAD
        return await dbSet
=======
        return await _userProfiles
            .Include(x => x.UserTypes)
>>>>>>> master
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.AccountId == accountId, cancellationToken);
    }

    public async Task<UserProfile?> GetUserCourses(Guid accountId, CancellationToken cancellationToken)
    {
        return await
            dbSet
                .Include(x => x.Account)
                .Include(x=>x.Courses)
                .ThenInclude(x=>x.Chapters)
                .ThenInclude(x=>x.SubChapters)
                .FirstOrDefaultAsync(x => x.AccountId == accountId,cancellationToken);
    }
}