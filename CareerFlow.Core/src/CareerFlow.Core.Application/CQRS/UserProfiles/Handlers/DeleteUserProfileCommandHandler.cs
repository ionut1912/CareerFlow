using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;

public class DeleteUserProfileCommandHandler
{
    private readonly ILogger<DeleteUserProfileCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProfileRepository _userProfileRepository;

    public DeleteUserProfileCommandHandler(IUserProfileRepository userProfileRepository,
        ILogger<DeleteUserProfileCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _userProfileRepository = userProfileRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userProfile = await _userProfileRepository.GetByIdAsync(request.Id, cancellationToken);
        if (userProfile == null)
        {
            _logger.LogError("UserProfile with Id {Id} not found", request.Id);
            throw new UserProfileNotFoundException($"Profilul cu id-ul {request.Id} nu a fost gasit");
        }

        _userProfileRepository.Delete(userProfile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("UserProfile with Id {Id} was deleted", request.Id);
    }
}