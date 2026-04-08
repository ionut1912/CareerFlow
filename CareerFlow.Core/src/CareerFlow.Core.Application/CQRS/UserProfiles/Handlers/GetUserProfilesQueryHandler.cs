using CareerFlow.Core.Application.CQRS.UserProfiles.Queries;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;

namespace CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;

public class GetUserProfilesQueryHandler
{
    private readonly IUserProfileRepository _userProfileRepository;


    public GetUserProfilesQueryHandler(IUserProfileRepository userProfileRepository)
    {
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        _userProfileRepository = userProfileRepository;
    }

    public async Task<List<UserProfileDto>> Handle(GetUserProfilesQuery request, CancellationToken cancellationToken)
    {
        var userProfiles = await _userProfileRepository.GetAllAsync(cancellationToken, up => up.Account);
        return userProfiles.ToDtos();
    }
}