using CareerFlow.Core.Domain.Entities;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Domain.Abstractions.Repositories;

public interface IUserProfileRepository : IGenericRepository<UserProfile>
{
    Task<UserProfile?> GetCurrentUserProfile(Guid accountId, CancellationToken cancellationToken);
    Task<UserProfile?> GetUserCourses(Guid accountId, CancellationToken cancellationToken);
}