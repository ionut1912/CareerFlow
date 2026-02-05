using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.TermAndConditions.Commands;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.TermAndConditions.Handlers;

public class AcceptTermAndConditionsCommandHandler : IRequestHandler<AcceptTermAndConditionsCommand, TermsAndConditionDto>
{
    private readonly ILogger<AcceptTermAndConditionsCommandHandler> _logger; 
    private readonly ITermsAndConditionsService _termsAndConditionsService;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptTermAndConditionsCommandHandler(
        ILogger<AcceptTermAndConditionsCommandHandler> logger,
        ITermsAndConditionsService termsAndConditionsService,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(termsAndConditionsService, nameof(termsAndConditionsService));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        _logger = logger;
        _termsAndConditionsService = termsAndConditionsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<TermsAndConditionDto> Handle(AcceptTermAndConditionsCommand request, CancellationToken cancellationToken = default)
    {    
        var termsAndConditions = await _termsAndConditionsService.GetByIdAsync(request.Id, cancellationToken);
        if (termsAndConditions is null)
        {
            _logger.LogWarning("Terms and Conditions with Id {Id} not found.", request.Id);
            throw new TermsAndConditionsNotFoundException($"Terms and Conditions with Id {request.Id} not found.");
        }

        termsAndConditions.Accept();
        _termsAndConditionsService.Update(termsAndConditions);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Terms and Conditions with Id {Id} accepted.", request.Id);
        return termsAndConditions.ToDto();

    }
}
