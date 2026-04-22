using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace CareerFlow.Core.Infrastructure.Gateways;

public class GithubPagesRequestsSender : IGithubPagesRequestsSender
{
    private readonly HttpClient _httpClient;
    private readonly string _privacyUrl;
    private readonly string _termsUrl;

    public GithubPagesRequestsSender(HttpClient httpClient, IOptions<LegalDocSettings> legalDocOptions)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(legalDocOptions);
        _httpClient = httpClient;
        string baseUrl = legalDocOptions.Value.GitHubPagesBaseUrl.TrimEnd('/');
        _privacyUrl = $"{baseUrl}/privacy.md";
        _termsUrl = $"{baseUrl}/terms.md";
    }

    public Task<HttpResponseMessage> GetContentAsync(string type, CancellationToken cancellationToken)
    {
        string url = type.Equals("privacy", StringComparison.OrdinalIgnoreCase)
            ? _privacyUrl
            : _termsUrl;

        return _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }
}
