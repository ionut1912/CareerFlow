using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Commands;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using Shared.Domain.Interfaces;

namespace CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Handlers;

public class UpdatePrivacyPolicyCommandHandler : IRequestHandler<UpdatePrivacyPolicyCommand, PrivacyPolicyDto>
{
    private readonly ILogger<UpdatePrivacyPolicyCommandHandler> _logger;
    private readonly IPrivacyPolicyService _privacyPolicyService;
    private readonly IUnitOfWork _unitOfWork;
    public UpdatePrivacyPolicyCommandHandler(
        ILogger<UpdatePrivacyPolicyCommandHandler> logger,
        IPrivacyPolicyService privacyPolicyService,
        IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(privacyPolicyService, nameof(privacyPolicyService));
        ArgumentNullException.ThrowIfNull(unitOfWork, nameof(unitOfWork));
        _logger = logger;
        _privacyPolicyService = privacyPolicyService;
        _unitOfWork = unitOfWork;
    }
    public async Task<PrivacyPolicyDto> Handle(UpdatePrivacyPolicyCommand request, CancellationToken cancellationToken = default)
    {
        var privacyPolicy = await _privacyPolicyService.GetByIdAsync(request.Id, cancellationToken);
        if (privacyPolicy is null)
        {
            _logger.LogWarning("Privacy policy with id {Id} not found.", request.Id);
            throw new PrivacyPolicyNotFoundException($"Privacy policy with id {request.Id} not found.");
        }
        privacyPolicy.UpdateContent(request.Content);
        _privacyPolicyService.Update(privacyPolicy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Privacy policy with id {Id} updated successfully.", request.Id);
        return privacyPolicy.ToPrivacyPolicyDto();
    }
}
