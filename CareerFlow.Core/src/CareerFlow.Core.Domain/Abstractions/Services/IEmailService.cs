namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IEmailService
{
    Task<bool> SendEmailWithTemplateAsync(string receiver, int templateId, Dictionary<string, string> placeholders);
}
