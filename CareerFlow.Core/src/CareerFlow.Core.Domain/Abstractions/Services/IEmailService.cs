namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface IEmailService
{
    Task<bool> SendEmailWithTemplateAsync(string to, int templateId, Dictionary<string, string> placeholders, CancellationToken cancellationToken);
}
