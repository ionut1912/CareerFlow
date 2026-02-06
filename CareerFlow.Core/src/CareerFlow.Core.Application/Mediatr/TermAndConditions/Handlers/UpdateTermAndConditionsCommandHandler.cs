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
        var termsAndConditions = await _termsAndConditionsService.GetByIdAsync(request.Id, cancellationToken);
        if (termsAndConditions is null)
        {
            _logger.LogWarning("Terms and Conditions with Id {Id} not found.", request.Id);
            throw new TermsAndConditionsNotFoundException($"Terms and Conditions with Id {request.Id} not found.");
        }
        termsAndConditions.UpdateContent(request.Content);
        _termsAndConditionsService.Update(termsAndConditions);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Terms and Conditions with Id {Id} updated successfully.", request.Id);
        await _cacheService.SetCacheValueAsync($"TermAndConditions_{termsAndConditions.Id}", termsAndConditions);
        return termsAndConditions.ToTermAndConditionsDto();
    }
}
