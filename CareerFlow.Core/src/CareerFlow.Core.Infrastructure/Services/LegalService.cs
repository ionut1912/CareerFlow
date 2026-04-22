using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Constants;
using CareerFlow.Core.Domain.Models.Legal;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Infrastructure.Services;

public partial class LegalService : ILegalService
{
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromHours(6);
    private readonly ICacheService _cache;
    private readonly ILogger<LegalService> _logger;
    private readonly IGithubPagesRequestsSender _requestsSender;

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
        string cacheKey = CacheKeyConstants.CacheKeyLegal(type);

        LegalDocumentResponse? cached = await _cache.GetAsync<LegalDocumentResponse>(cacheKey);
        if (cached is not null)
        {
            LogDocumentServedFromCache(_logger, type);
            return cached;
        }

        HttpResponseMessage response = await _requestsSender.GetContentAsync(type, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            LogGithubPagesError(_logger, (int)response.StatusCode, type);
            return null;
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        var document = new LegalDocumentResponse(content, "GitHub Pages", DateTime.UtcNow);

        await _cache.SetAsync(cacheKey, document, _cacheDuration);
        LogDocumentFetchedAndCached(_logger, type);

        return document;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Legal document '{Type}' served from cache")]
    private static partial void LogDocumentServedFromCache(ILogger logger, string type);

    [LoggerMessage(Level = LogLevel.Warning, Message = "GitHub Pages returned {StatusCode} for document type '{Type}'")]
    private static partial void LogGithubPagesError(ILogger logger, int statusCode, string type);

    [LoggerMessage(Level = LogLevel.Information, Message = "Legal document '{Type}' fetched from GitHub Pages and cached")]
    private static partial void LogDocumentFetchedAndCached(ILogger logger, string type);
}
