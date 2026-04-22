using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Infrastructure.Services;

public partial class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IMailClient _mailClient;

    public EmailService(IMailClient mailClient, ILogger<EmailService> logger)
    {
        ArgumentNullException.ThrowIfNull(mailClient);
        ArgumentNullException.ThrowIfNull(logger);
        _mailClient = mailClient;
        _logger = logger;
    }

    public async Task<bool> SendEmailWithTemplateAsync(string receiver, int templateId,
        Dictionary<string, string> placeholders)
    {
        if (string.IsNullOrWhiteSpace(receiver))
        {
            LogEmptyReceiver();
            return false;
        }

        try
        {
            bool success = await _mailClient.SendTemplatedEmailAsync(receiver, templateId, placeholders);

            if (success)
            {
                LogEmailSent(templateId);
                return true;
            }

            LogEmailFailed();
            return false;
        }
        catch (InvalidOperationException ex)
        {
            LogEmailException(templateId, ex);
            return false;
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Receiver email address is empty")]
    private partial void LogEmptyReceiver();

    [LoggerMessage(Level = LogLevel.Information, Message = "Email sent successfully using template {TemplateId}")]
    private partial void LogEmailSent(int templateId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to send email via provider.")]
    private partial void LogEmailFailed();

    [LoggerMessage(Level = LogLevel.Error, Message = "Exception occurred while sending email with template {TemplateId}")]
    private partial void LogEmailException(int templateId, Exception ex);
}
