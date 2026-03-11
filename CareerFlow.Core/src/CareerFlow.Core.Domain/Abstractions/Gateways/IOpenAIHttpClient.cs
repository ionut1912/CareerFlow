namespace CareerFlow.Core.Domain.Abstractions.Gateways;

public interface IOpenAIHttpClient
{
    Task<TResponse> CreateAsync<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        CancellationToken ct = default);
}