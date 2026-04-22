using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;

public partial class UpdateUserProfileCommandHandler
{
    private readonly ILogger<UpdateUserProfileCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProfileRepository _userProfileRepository;

    public UpdateUserProfileCommandHandler(IUserProfileRepository userProfileRepository,
        ILogger<UpdateUserProfileCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _userProfileRepository = userProfileRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        UserProfile? userProfile = await _userProfileRepository.GetByIdAsync(request.Id, cancellationToken);
        if (userProfile == null)
        {
            LogUserProfileNotFound(_logger, request.Id);
            throw new UserProfileNotFoundException($"Profilul cu id-ul {request.Id} nu a fost gasit");
        }

        var learningType = LearningType.FromString(request.LearningType);
        var userTypes = request.UserTypes.Select(UserType.FromString).ToList();
        userProfile.Update(learningType, userTypes, request.Domain);
        _userProfileRepository.Update(userProfile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        LogUserProfileUpdated(_logger, request.Id);
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "UserProfile with Id {Id} not found")]
    private static partial void LogUserProfileNotFound(ILogger logger, Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "UserProfile with Id {Id} updated")]
    private static partial void LogUserProfileUpdated(ILogger logger, Guid id);
}
