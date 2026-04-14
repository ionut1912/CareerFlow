using CareerFlow.Core.Application.CQRS.UserProfiles.Queries;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;

public class GetUserProfileByIdQueryHandler
{
    private readonly ILogger<GetUserProfileByIdQueryHandler> _logger;
    private readonly IUserProfileRepository _userProfileRepository;

    public GetUserProfileByIdQueryHandler(IUserProfileRepository userProfileRepository,
        ILogger<GetUserProfileByIdQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        ArgumentNullException.ThrowIfNull(logger);
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _userProfileRepository.GetByIdAsync(request.Id, cancellationToken, up => up.Account!);
        if (userProfile is not null) return userProfile.ToDto();
        _logger.LogError("Profilul cu id-ul {Id} not foud", request.Id);
        throw new UserProfileNotFoundException($"Profilul cu id-ul {request.Id} nu a fost gasit");
    }
}