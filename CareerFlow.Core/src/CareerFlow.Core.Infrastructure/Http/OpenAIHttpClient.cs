using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Infrastructure.Http;

public sealed class OpenAIHttpClient : IOpenAIHttpClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly ILogger<OpenAIHttpClient> _logger;

    public OpenAIHttpClient(HttpClient http, ILogger<OpenAIHttpClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<TResponse> CreateAsync<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        CancellationToken ct = default)
    {
        _logger.LogDebug("POST {Endpoint}", endpoint);

        var response = await _http.PostAsJsonAsync(endpoint, body, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("OpenAI error {StatusCode}: {Error}", response.StatusCode, error);
            throw new OpenAIException((int)response.StatusCode, error);
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct)
               ?? throw new OpenAIException(0, "Empty response received from OpenAI");
    }
}