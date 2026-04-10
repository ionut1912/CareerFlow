using CareerFlow.Core.Application.CQRS.UserProfiles.Queries;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;

public class GetCurrentUserCoursesQueryHandler
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ILogger<GetCurrentUserCoursesQueryHandler> _logger;

    public GetCurrentUserCoursesQueryHandler(IUserProfileRepository userProfileRepository, ILogger<GetCurrentUserCoursesQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        ArgumentNullException.ThrowIfNull(logger);
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task<UserProfileDto> Handle(GetCurrentUserCoursesQuery request, CancellationToken cancellationToken)
    {



        var currentUserProfileWithCourses =
            await _userProfileRepository.GetUserCourses(request.AccountId, cancellationToken);
        if (currentUserProfileWithCourses is not null) return currentUserProfileWithCourses.ToDto();
        _logger.LogError("Profilul cu id {AccountId} nu a fost gasit",request.AccountId);
        throw new AccountNotFoundException($"Profilul cu id {request.AccountId} nu a fost gasit");
    }
}