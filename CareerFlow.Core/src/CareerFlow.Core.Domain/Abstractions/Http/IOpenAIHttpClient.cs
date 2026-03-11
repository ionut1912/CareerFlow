namespace CareerFlow.Core.Domain.Abstractions.Http;

public interface IOpenAIHttpClient
{
    Task<TResponse> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest body,
        CancellationToken ct = default);
}