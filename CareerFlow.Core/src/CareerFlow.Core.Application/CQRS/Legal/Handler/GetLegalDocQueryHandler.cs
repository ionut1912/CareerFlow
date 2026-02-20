using System.Text.Json;
using CareerFlow.Core.Application.CQRS.Legal.Query;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Mappings;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.CQRS.Legal.Handler;

public class GetLegalDocQueryHandler
{
    private readonly ICacheService _cacheService;
    private readonly ILegalDocRepository _legalDocRepository;
    private readonly ILogger<GetLegalDocQueryHandler> _logger;

    public GetLegalDocQueryHandler(ILegalDocRepository legalDocRepository, ICacheService cacheService,
        ILogger<GetLegalDocQueryHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(legalDocRepository);
        ArgumentNullException.ThrowIfNull(cacheService);
        ArgumentNullException.ThrowIfNull(logger);
        _legalDocRepository = legalDocRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<LegalDocDto> Handle(GetLegalDocQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"LegalDoc_{query.Type}";
        var cachedLegalDoc = await _cacheService.GetCacheValueAsync<LegalDocDto>(cacheKey);
        if (cachedLegalDoc != null)
        {
            _logger.LogInformation("Documentul cu tipul {Type} a fost luat din cache ,resultat {cacheLegalDocDto}.",
                query.Type,
                JsonSerializer.Serialize(cachedLegalDoc, new JsonSerializerOptions { WriteIndented = true }));
            return cachedLegalDoc;
        }

        var legalDoc = await _legalDocRepository.GetLegalDocByTypeAsync(query.Type, cancellationToken);
        if (legalDoc == null)
        {
            _logger.LogError("Nu exista un document pentru tipul {Type}.", query.Type);
            throw new LegalDocNotFoundException($"Nu exista document pentru tipul {query.Type}");
        }

        var legalDocDto = legalDoc.ToDto();
        await _cacheService.SetCacheValueAsync(cacheKey, legalDocDto);
        _logger.LogInformation("Documentul cu tipul {Type} preluat din baza de data,rezultat {legalDocDto}", query.Type,
            JsonSerializer.Serialize(legalDocDto, new JsonSerializerOptions { WriteIndented = true }));
        return legalDocDto;
    }
}