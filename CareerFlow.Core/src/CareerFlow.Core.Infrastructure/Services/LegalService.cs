using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace CareerFlow.Core.Infrastructure.Services;

public class LegalService : ILegalService
{
    private readonly IGithubPagesRequestsSender _requestsSender;
    public LegalService(IGithubPagesRequestsSender requestsSender)
    {
        ArgumentNullException.ThrowIfNull(requestsSender);
        _requestsSender = requestsSender;
    }

    public async Task<LegalDocumentResponse?> GetDocumentAsync(string type, CancellationToken cancellationToken)
    {
        var response = await _requestsSender.GetContentAsync(type, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return new LegalDocumentResponse(content, "GitHub Pages", DateTime.UtcNow);
    }
}