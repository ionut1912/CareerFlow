using CareerFlow.Core.Application.CQRS.UserProfiles.Queries;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;

using JetBrains.Annotations;

namespace CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;

[UsedImplicitly]
public class GetUserProfilesQueryHandler
{
    private readonly IUserProfileRepository _userProfileRepository;

    public GetUserProfilesQueryHandler(IUserProfileRepository userProfileRepository)
    {
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        _userProfileRepository = userProfileRepository;
    }

    [UsedImplicitly]
    public async Task<List<UserProfileDto>> Handle(GetUserProfilesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<UserProfile> userProfiles =
            await _userProfileRepository.GetAllAsync(cancellationToken, up => up.Account!);
        return userProfiles.ToDtos();
    }
}
