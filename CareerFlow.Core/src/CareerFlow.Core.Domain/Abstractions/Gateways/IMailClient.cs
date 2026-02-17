namespace CareerFlow.Core.Domain.Abstractions.Gateways;

public interface IMailClient
{
    Task<bool> SendTemplatedEmailAsync(string to, int templateId, Dictionary<string, string> model, CancellationToken cancellationToken);
}