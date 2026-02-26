namespace CareerFlow.Core.Domain.Abstractions.Gateways;

public interface IGithubPagesRequestsSender
{
    Task<HttpResponseMessage> GetContentAsync(string type, CancellationToken cancellationToken);
}