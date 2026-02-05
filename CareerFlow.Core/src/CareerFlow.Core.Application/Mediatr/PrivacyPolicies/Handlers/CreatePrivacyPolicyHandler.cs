using CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Commands;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Handlers;

public class CreatePrivacyPolicyHandler : IRequestHandler<CreatePrivacyPolicyCommand, Guid>
{
    private readonly ILogger<CreatePrivacyPolicyHandler> _logger;
    private readonly IPrivacyPolicyService _privacyPolicyService;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePrivacyPolicyHandler(ILogger<CreatePrivacyPolicyHandler> logger, IPrivacyPolicyService privacyPolicyService, IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(privacyPolicyService, nameof(privacyPolicyService));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        _logger = logger;
        _privacyPolicyService = privacyPolicyService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreatePrivacyPolicyCommand request, CancellationToken cancellationToken = default)
    {
        var privacyPolicy = PrivacyPolicy.CreateContent(request.Content);
        await _privacyPolicyService.AddAsync(privacyPolicy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Privacy policy created with ID: {PrivacyPolicyId}", privacyPolicy.Id);
        return privacyPolicy.Id;
    }
}
