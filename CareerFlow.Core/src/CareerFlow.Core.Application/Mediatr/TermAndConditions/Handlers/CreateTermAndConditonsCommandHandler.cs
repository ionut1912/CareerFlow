using CareerFlow.Core.Application.Mediatr.TermAndConditions.Commands;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.TermAndConditions.Handlers;

public class CreateTermAndConditonsCommandHandler : IRequestHandler<CreateTermAndConditionsCommand, Guid>
{
    private readonly ILogger<CreateTermAndConditonsCommandHandler> _logger;
    private readonly ITermsAndConditionsRepository _termsAndConditionsService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    
    public CreateTermAndConditonsCommandHandler(ILogger<CreateTermAndConditonsCommandHandler> logger, ITermsAndConditionsRepository termsAndConditionsService, IUnitOfWork unitOfWork,ICacheService cacheService)
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

    public async Task<Guid> Handle(CreateTermAndConditionsCommand request, CancellationToken cancellationToken = default)
    {
        var termAndConditions = TermsAndCondition.CreateContent(request.Content);
        await _termsAndConditionsService.AddAsync(termAndConditions, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Term and Conditions created with Id: {TermAndConditionsId}", termAndConditions.Id);
        await _cacheService.SetCacheValueAsync("TermAndConditions", termAndConditions);
        return termAndConditions.Id;
    }
}
