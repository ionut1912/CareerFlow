using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Handlers;

public class UpdatePrivacyPolicyCommandHandler : IRequestHandler<UpdatePrivacyPolicyCommand, PrivacyPolicyDto>
{
    private readonly ILogger<UpdatePrivacyPolicyCommandHandler> _logger;
    private readonly IPrivacyPolicyRepository _privacyPolicyService;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public UpdatePrivacyPolicyCommandHandler(
        IAccountRepository accountRepository,
        ILogger<UpdatePrivacyPolicyCommandHandler> logger,
        IPrivacyPolicyRepository privacyPolicyService,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(privacyPolicyService, nameof(privacyPolicyService));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));
        _logger = logger;
        _privacyPolicyService = privacyPolicyService;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _accountRepository = accountRepository;
    }
    public async Task<PrivacyPolicyDto> Handle(UpdatePrivacyPolicyCommand request, CancellationToken cancellationToken = default)
    {
        var users = await _accountRepository.GetAllAsync(cancellationToken);
        var privacyPolicy = await _privacyPolicyService.GetAllAsync(cancellationToken);
        var privacyPolicyEntity = privacyPolicy.FirstOrDefault();
        if (privacyPolicyEntity is null)
        {
            _logger.LogWarning("Privacy policy  not found.");
            throw new PrivacyPolicyNotFoundException($"Privacy policy with id  not found.");
        }
        privacyPolicyEntity.UpdateContent(request.Content);
        _privacyPolicyService.Update(privacyPolicyEntity);

        foreach (var user in users)
        {
            user.UpdatePrivacyPolicy();
            _accountRepository.Update(user);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Privacy policy updated successfully.");
        await _cacheService.SetCacheValueAsync("PrivacyPolicy", privacyPolicyEntity);
        return privacyPolicyEntity.ToPrivacyPolicyDto();
    }
}
