using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IMailClient _mailClient;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IMailClient mailClient, ILogger<EmailService> logger)
    {
        ArgumentNullException.ThrowIfNull(mailClient);
        ArgumentNullException.ThrowIfNull(logger);
        _mailClient = mailClient;
        _logger = logger;
    }

    public async Task<bool> SendEmailWithTemplateAsync(string to, int templateId, Dictionary<string, string> templateModel, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Email sending cancelled before execution.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogError("Receiver email address is empty");
            return false;
        }

        try
        {
            var success = await _mailClient.SendTemplatedEmailAsync(to, templateId, templateModel, cancellationToken);

            if (success)
            {
                _logger.LogInformation("Email sent successfully using template {TemplateId}", templateId);
                return true;
            }

            _logger.LogError("Failed to send email via provider.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email with template {TemplateId}", templateId);
            return false;
        }
    }
}