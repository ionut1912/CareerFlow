using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Models;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace CareerFlow.Core.Infrastructure.Services;

public class LegalService : ILegalService
{
    private readonly HttpClient _httpClient;
    private readonly LegalDocSettings _legalDocSettings;

    public LegalService(HttpClient httpClient, IOptions<LegalDocSettings> legalDocOptions)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(legalDocOptions);
        _httpClient = httpClient;
        _legalDocSettings = legalDocOptions.Value;
    }

    public async Task<LegalDocumentResponse?> GetDocumentAsync(string type, CancellationToken cancellationToken)
    {
        var baseUrl = _legalDocSettings.GitHubPagesBaseUrl;
        var fileName = type.ToLower() == "privacy" ? "privacy.md" : "terms.md";
        var url = $"{baseUrl.TrimEnd('/')}/{fileName}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return new LegalDocumentResponse(content, "GitHub Pages", DateTime.UtcNow);
    }
}