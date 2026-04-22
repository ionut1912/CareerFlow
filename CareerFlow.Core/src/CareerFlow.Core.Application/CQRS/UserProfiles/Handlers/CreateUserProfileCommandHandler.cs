using CareerFlow.Core.Application.CQRS.UserProfiles.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.UserProfiles.Handlers;

public partial class CreateUserProfileCommandHandler
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<CreateUserProfileCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserProfileRepository _userProfileRepository;

    public CreateUserProfileCommandHandler(IUserProfileRepository userProfileRepository,
        IAccountRepository accountRepository, ILogger<CreateUserProfileCommandHandler> logger, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(userProfileRepository);
        ArgumentNullException.ThrowIfNull(accountRepository);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(unitOfWork);

        _userProfileRepository = userProfileRepository;
        _accountRepository = accountRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
    {
        Account? account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);

        if (account == null)
        {
            LogAccountNotFound(request.AccountId);
            throw new AccountNotFoundException($"Contul cu id-ul {request.AccountId} nu a fost gasit");
        }

        var learningType = LearningType.FromString(request.LearningType);
        var userTypes = request.UserTypes.Select(UserType.FromString).ToList();
        var userProfile = UserProfile.Create(request.AccountId, learningType, userTypes, request.Domain);

        await _userProfileRepository.AddAsync(userProfile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return userProfile.Id;
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Contul cu id-ul {Id} nu a fost gasit")]
    private partial void LogAccountNotFound(Guid id);
}
