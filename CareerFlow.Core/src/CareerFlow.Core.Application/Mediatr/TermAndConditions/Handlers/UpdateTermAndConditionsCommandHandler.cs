using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.TermAndConditions.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.TermAndConditions.Handlers;

public class UpdateTermAndConditionsCommandHandler : IRequestHandler<UpdateTermsAndConditionsCommand, TermsAndConditionDto>
{
    private readonly ILogger<UpdateTermAndConditionsCommandHandler> _logger;
    private readonly ITermsAndConditionsRepository _termsAndConditionsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public UpdateTermAndConditionsCommandHandler(
        ILogger<UpdateTermAndConditionsCommandHandler> logger,
        ITermsAndConditionsRepository termsAndConditionsService,
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(termsAndConditionsService, nameof(termsAndConditionsService));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));
        _logger = logger;
        _termsAndConditionsService = termsAndConditionsService;
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<TermsAndConditionDto> Handle(UpdateTermsAndConditionsCommand request, CancellationToken cancellationToken = default)
    {
        var termsAndConditions = await _termsAndConditionsService.GetAllAsync(cancellationToken);
        var termsAndConditionsEntity = termsAndConditions.FirstOrDefault();
        if (termsAndConditionsEntity is null)
        {
            _logger.LogWarning("Terms and Conditions not found.");
            throw new TermsAndConditionsNotFoundException("Terms and Conditions with Id  not found.");
        }
        termsAndConditionsEntity.UpdateContent(request.Content);
        _termsAndConditionsService.Update(termsAndConditionsEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Terms and Conditions updated successfully.");
        await _cacheService.SetCacheValueAsync("TermAndConditions", termsAndConditions);
        return termsAndConditionsEntity.ToTermAndConditionsDto();
    }
}
