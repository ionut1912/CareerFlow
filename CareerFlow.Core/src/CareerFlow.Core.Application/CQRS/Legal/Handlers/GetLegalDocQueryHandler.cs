using CareerFlow.Core.Application.CQRS.Legal.Query;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CareerFlow.Core.Application.CQRS.Legal.Handlers;

public class GetLegalDocQueryHandler
{
    private readonly ILegalDocRepository _legalDocRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetLegalDocQueryHandler> _logger;

    public GetLegalDocQueryHandler(ILegalDocRepository legalDocRepository,ICacheService cacheService,ILogger<GetLegalDocQueryHandler> logger)
    {
        _legalDocRepository = legalDocRepository ?? throw new ArgumentNullException(nameof(legalDocRepository));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LegalDocDto?> Handle(GetLegalDocQuery query, CancellationToken ct)
    {
        var cacheKey = $"LegalDoc_{query.Type}";
        var cachedLegalDoc = await _cacheService.GetCacheValueAsync<LegalDoc>(cacheKey);
        if (cachedLegalDoc != null)
        {
            var cacheLegalDocDto = cachedLegalDoc.ToDto();
            _logger.LogInformation("Legal document of type {Type} retrieved from cache ,result {cacheLegalDocDto}.", query.Type,
                JsonSerializer.Serialize(cacheLegalDocDto,new JsonSerializerOptions { WriteIndented=true}));
            return cacheLegalDocDto;
        }
        var legalDoc = await _legalDocRepository.GetLegalDocByTypeAsync(query.Type, ct);

        if (legalDoc == null)
        {
            _logger.LogError("No legal document found for type {Type}.", query.Type);
            throw new LegalDocNotFoundException($"No legal document found for type {query.Type}");

        }
        var legalDocDto = legalDoc.ToDto();
        await _cacheService.SetCacheValueAsync(cacheKey, legalDocDto);
        _logger.LogInformation("Legal document of type {Type} retrieved from database,result {cacheLegalDocDto}", query.Type,
            JsonSerializer.Serialize(legalDocDto,new JsonSerializerOptions { WriteIndented=true}));
        return legalDocDto;
    }
}
