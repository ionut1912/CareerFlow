using CareerFlow.Core.Application.Mediatr.TermAndConditions.Commands;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.TermAndConditions.Handlers;

public class CreateTermAndConditonsCommandHandler : IRequestHandler<CreateTermAndConditionsCommand, Guid>
{
    public ILogger<CreateTermAndConditonsCommandHandler> _logger;
    public ITermsAndConditionsService _termsAndConditionsService;
    public IUnitOfWork _unitOfWork;

    public CreateTermAndConditonsCommandHandler(ILogger<CreateTermAndConditonsCommandHandler> logger,ITermsAndConditionsService termsAndConditionsService,IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(termsAndConditionsService, nameof(termsAndConditionsService));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        _logger = logger;
        _termsAndConditionsService = termsAndConditionsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTermAndConditionsCommand request, CancellationToken cancellationToken = default)
    {
        var termAndConditions= TermsAndCondition.CreateContent(request.Content);
        await _termsAndConditionsService.AddAsync(termAndConditions, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Term and Conditions created with Id: {TermAndConditionsId}", termAndConditions.Id);
        return termAndConditions.Id;
    }
}
