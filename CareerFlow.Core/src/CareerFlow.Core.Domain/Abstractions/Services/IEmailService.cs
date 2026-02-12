namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IEmailService
{
    Task<bool> SendEmailWithTemplateAsync(string to,  string templateAlias, Dictionary<string, string> placeholders);
}
