using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;

using Microsoft.EntityFrameworkCore;

using Shared.Infra.Services;

namespace CareerFlow.Core.Infrastructure.Persistence.Repositories;

public class UserProfileRepository(DbSet<UserProfile> dbSet)
    : GenericRepository<UserProfile>(dbSet), IUserProfileRepository
{
    private readonly DbSet<UserProfile> _userProfiles = dbSet;

    public async Task<UserProfile?> GetCurrentUserProfile(Guid accountId, CancellationToken cancellationToken)
    {
        return await _userProfiles
            .Include(x => x.UserTypes)
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.AccountId == accountId, cancellationToken);
    }

    public async Task<UserProfile?> GetUserCourses(Guid accountId, CancellationToken cancellationToken)
    {
        return await
            _userProfiles
                .Include(x => x.Account)
                .Include(x => x.Courses)
                .ThenInclude(x => x.Chapters.OrderBy(chapter => chapter.Day))
                .ThenInclude(x => x.SubChapters)
                .FirstOrDefaultAsync(x => x.AccountId == accountId, cancellationToken);
    }
}
