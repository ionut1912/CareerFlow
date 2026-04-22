using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;

public partial class DeleteUserProfileCommandHandler
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<DeleteUserProfileCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProfileRepository _userProfileRepository;

    public DeleteUserProfileCommandHandler(IUserProfileRepository userProfileRepository,
        ILogger<DeleteUserProfileCommandHandler> logger, IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        ArgumentNullException.ThrowIfNull(cacheService);
        _userProfileRepository = userProfileRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task Handle(DeleteUserProfileCommand request, CancellationToken cancellationToken)
    {
        UserProfile? userProfile = await _userProfileRepository.GetByIdAsync(request.Id, cancellationToken);
        if (userProfile == null)
        {
            LogProfileNotFound(request.Id);
            throw new UserProfileNotFoundException($"Profilul cu id-ul {request.Id} nu a fost gasit");
        }

        _userProfileRepository.Delete(userProfile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        LogProfileDeleted(request.Id);
        await _cacheService.RemoveByPatternAsync("course:analyze");
        await _cacheService.RemoveByPatternAsync("course:chapters");
        await _cacheService.RemoveByPatternAsync("course:skeleton");
        await _cacheService.RemoveByPatternAsync("course:expand:");
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "UserProfile with Id {Id} not found")]
    private partial void LogProfileNotFound(Guid id);

    [LoggerMessage(Level = LogLevel.Information, Message = "UserProfile with Id {Id} was deleted")]
    private partial void LogProfileDeleted(Guid id);
}
