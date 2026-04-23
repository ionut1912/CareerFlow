namespace CareerFlow.Core.Domain.Abstractions.Gateways;

public interface IMailClient
{
    Task<bool> SendTemplatedEmailAsync(string receiver, int templateId, Dictionary<string, string> model);
}
