using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Query;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using System.Text.Json;

namespace CareerFlow.Core.Application.Mediatr.PrivacyPolicies.Handlers;

public class GetPrivacyPolicyQueryHandler : IRequestHandler<GetPrivacyPolicyQuery, PrivacyPolicyDto>
{
    private readonly ILogger<GetPrivacyPolicyQueryHandler> _logger;
    private readonly IPrivacyPolicyRepository _privacyPolicyService;
    private readonly ICacheService _cacheService;

    public GetPrivacyPolicyQueryHandler(ILogger<GetPrivacyPolicyQueryHandler> logger, IPrivacyPolicyRepository privacyPolicyService, ICacheService cacheService)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(privacyPolicyService, nameof(privacyPolicyService));
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));

        _logger = logger;
        _privacyPolicyService = privacyPolicyService;
        _cacheService = cacheService;
    }

    public async Task<PrivacyPolicyDto> Handle(GetPrivacyPolicyQuery request, CancellationToken cancellationToken = default)
    {
        var privacyFromCache = await _cacheService.GetCacheValueAsync<PrivacyPolicy>("PrivacyPolicy");

        if (privacyFromCache != null)
        {
            var privacyPolicyDtoCache = privacyFromCache.ToPrivacyPolicyDto();
            _logger.LogInformation("Privacy policy  retrieved successfully from cache,result {privacyPolicyDto}.",
                 JsonSerializer.Serialize(privacyPolicyDtoCache, new JsonSerializerOptions { WriteIndented = true }));
            return privacyPolicyDtoCache;
        }
        var privacyPolicy = await _privacyPolicyService.GetAllAsync(cancellationToken);
        var privacyPolicyEntity = privacyPolicy.FirstOrDefault();
        if (privacyPolicyEntity == null)
        {
            _logger.LogWarning("Privacy policy not found.");
            throw new PrivacyPolicyNotFoundException("Privacy policy with ID  not found.");
        }

        var privacyPolicyDto = privacyPolicyEntity.ToPrivacyPolicyDto();
        _logger.LogInformation("Privacy policy  retrieved successfully from database,result {privacyPolicyDto}.",
             JsonSerializer.Serialize(privacyPolicyDto, new JsonSerializerOptions { WriteIndented = true }));
        await _cacheService.SetCacheValueAsync("PrivacyPolicy", privacyPolicyEntity);
        return privacyPolicyDto;
    }
}
