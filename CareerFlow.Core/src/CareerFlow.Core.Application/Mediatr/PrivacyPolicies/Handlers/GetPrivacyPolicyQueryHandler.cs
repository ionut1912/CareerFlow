using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Query;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using System.Text.Json;

namespace CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Handlers;

public class GetPrivacyPolicyQueryHandler : IRequestHandler<GetPrivacyPolicyQuery, PrivacyPolicyDto>
{
    private readonly ILogger<GetPrivacyPolicyQueryHandler> _logger;
    private readonly IPrivacyPolicyService _privacyPolicyService;

    public GetPrivacyPolicyQueryHandler(ILogger<GetPrivacyPolicyQueryHandler> logger, IPrivacyPolicyService privacyPolicyService)
    {
        _logger = logger;
        _privacyPolicyService = privacyPolicyService;
    }

    public async Task<PrivacyPolicyDto> Handle(GetPrivacyPolicyQuery request, CancellationToken cancellationToken = default)
    {
        var privacyPolicy = await _privacyPolicyService.GetByIdAsync(request.Id, cancellationToken);
        if (privacyPolicy == null)
        {
            _logger.LogWarning("Privacy policy with ID {PrivacyPolicyId} not found.", request.Id);
            throw new PrivacyPolicyNotFoundException($"Privacy policy with ID {request.Id} not found.");
        }
        var privacyPolicyDto = privacyPolicy.ToPrivacyPolicyDto();
        _logger.LogInformation("Privacy policy with ID {PrivacyPolicyId} retrieved successfully,result {privacyPolicyDto}.",
            request.Id, JsonSerializer.Serialize(privacyPolicyDto, new JsonSerializerOptions { WriteIndented = true }));
        return privacyPolicyDto;
    }
}
