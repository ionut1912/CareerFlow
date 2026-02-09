using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Application.Mediatr.TermAndConditions.Query;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using Shared.Application.Mediator;
using System.Text.Json;

namespace CareerFlow.Core.Application.Mediatr.TermAndConditions.Handlers;

public class GetTermAndConditionsQueryHandler : IRequestHandler<GetTermAndConditionsQuery, TermsAndConditionDto>
{
    private readonly ILogger<GetTermAndConditionsQueryHandler> _logger;
    private readonly ITermsAndConditionsRepository _termsAndConditionsService;
    private readonly ICacheService _cacheService;

    public GetTermAndConditionsQueryHandler(ILogger<GetTermAndConditionsQueryHandler> logger, ITermsAndConditionsRepository termsAndConditionsService, ICacheService cacheService)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(cacheService, nameof(cacheService));
        ArgumentNullException.ThrowIfNull(termsAndConditionsService, nameof(termsAndConditionsService));
        _logger = logger;
        _termsAndConditionsService = termsAndConditionsService;
        _cacheService = cacheService;
    }

    public async Task<TermsAndConditionDto> Handle(GetTermAndConditionsQuery request, CancellationToken cancellationToken = default)
    {
        var termsAndConditionsFromCache = await _cacheService.GetCacheValueAsync<TermsAndCondition>("TermAndConditions");
        if (termsAndConditionsFromCache is not null)
        {
            var termsAndConditionsFromCacheDto = termsAndConditionsFromCache.ToTermAndConditionsDto();
            _logger.LogInformation("Terms and Conditions retrieved successfully from cache,result {termAndConditionsDto}.",
               JsonSerializer.Serialize(termsAndConditionsFromCacheDto, new JsonSerializerOptions { WriteIndented = true }));
            return termsAndConditionsFromCacheDto;
        }

        var termsAndConditions = await _termsAndConditionsService.GetAllAsync(cancellationToken);
        var termsAndConditionsEntity = termsAndConditions.FirstOrDefault();
        if (termsAndConditionsEntity is null)
        {
            _logger.LogWarning("Terms and Conditions not found.");
            throw new TermsAndConditionsNotFoundException("Terms and Conditions with Id not found.");
        }
        var termsAndConditionsDto = termsAndConditionsEntity.ToTermAndConditionsDto();
        _logger.LogInformation("Terms and Conditions retrieved successfully from database,result {termAndConditionsDto}.",
            JsonSerializer.Serialize(termsAndConditionsDto, new JsonSerializerOptions { WriteIndented = true }));
        await _cacheService.SetCacheValueAsync("TermAndConditions", termsAndConditionsEntity);
        return termsAndConditionsDto;
    }
}
