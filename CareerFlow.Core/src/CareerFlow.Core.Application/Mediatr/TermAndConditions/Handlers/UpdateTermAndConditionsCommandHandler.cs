using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.TermAndConditions.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.TermAndConditions.Handlers;

public class UpdateTermAndConditionsCommandHandler : IRequestHandler<UpdateTermsAndConditionsCommand, TermsAndConditionDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<UpdateTermAndConditionsCommandHandler> _logger;
    private readonly ITermsAndConditionsRepository _termsAndConditionsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public UpdateTermAndConditionsCommandHandler(
        IAccountRepository accountRepository,
        ILogger<UpdateTermAndConditionsCommandHandler> logger,
        ITermsAndConditionsRepository termsAndConditionsService,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        ArgumentNullException.ThrowIfNull(accountRepository, nameof(accountRepository));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(termsAndConditionsService, nameof(termsAndConditionsService));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));
        _accountRepository = accountRepository;
        _logger = logger;
        _termsAndConditionsService = termsAndConditionsService;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<TermsAndConditionDto> Handle(UpdateTermsAndConditionsCommand request, CancellationToken cancellationToken = default)
    {
        var users = await _accountRepository.GetAllAsync(cancellationToken);
        var termsAndConditions = await _termsAndConditionsService.GetAllAsync(cancellationToken);
        var termsAndConditionsEntity = termsAndConditions.FirstOrDefault();
        if (termsAndConditionsEntity is null)
        {
            _logger.LogWarning("Terms and Conditions not found.");
            throw new TermsAndConditionsNotFoundException("Terms and Conditions with Id  not found.");
        }
        termsAndConditionsEntity.UpdateContent(request.Content);
        _termsAndConditionsService.Update(termsAndConditionsEntity);

        foreach (var user in users)
        {
            user.UpdateTerns();
            _accountRepository.Update(user);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Terms and Conditions updated successfully.");
        await _cacheService.SetCacheValueAsync("TermAndConditions", termsAndConditions);
        return termsAndConditionsEntity.ToTermAndConditionsDto();
    }
}
