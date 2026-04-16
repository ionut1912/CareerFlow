using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models.Legal;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Infrastructure.Services;

public class LegalService : ILegalService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);

    private readonly IGithubPagesRequestsSender _requestsSender;
    private readonly ICacheService _cache;
    private readonly ILogger<LegalService> _logger;

    public LegalService(
        IGithubPagesRequestsSender requestsSender,
        ICacheService cache,
        ILogger<LegalService> logger)
    {
        ArgumentNullException.ThrowIfNull(requestsSender);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);
        _requestsSender = requestsSender;
        _cache = cache;
        _logger = logger;
    }

    public async Task<LegalDocumentResponse?> GetDocumentAsync(string type, CancellationToken cancellationToken)
    {
        var cacheKey = $"legal:{type.ToLowerInvariant()}";

        var cached = await _cache.GetAsync<LegalDocumentResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Legal document '{Type}' served from cache", type);
            return cached;
        }

        var response = await _requestsSender.GetContentAsync(type, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("GitHub Pages returned {StatusCode} for document type '{Type}'",
                (int)response.StatusCode, type);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var document = new LegalDocumentResponse(content, "GitHub Pages", DateTime.UtcNow);

        await _cache.SetAsync(cacheKey, document, CacheDuration, cancellationToken);
        _logger.LogInformation("Legal document '{Type}' fetched from GitHub Pages and cached", type);

        return document;
    }
}