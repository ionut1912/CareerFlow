using CareerFlow.Core.Application.CQRS.UserProfiles.Queries;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;

public partial class GetCurrentUserProfileQueryHandler
{
    private readonly ILogger<GetCurrentUserProfileQueryHandler> _logger;
    private readonly IUserProfileRepository _userProfileRepository;

    public GetCurrentUserProfileQueryHandler(IUserProfileRepository userProfileRepository,
        ILogger<GetCurrentUserProfileQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        ArgumentNullException.ThrowIfNull(logger);
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task<UserProfileDto> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        UserProfile? userProfile = await _userProfileRepository.GetCurrentUserProfile(request.AccountId, cancellationToken);

        if (userProfile is not null) return userProfile.ToDto();

        LogProfileNotFound(request.AccountId);
        throw new UserProfileNotFoundException($"Profilul cu id-ul {request.AccountId} nu a fost gasit");
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Profilul cu id-ul {Id} not found")]
    private partial void LogProfileNotFound(Guid id);
}
