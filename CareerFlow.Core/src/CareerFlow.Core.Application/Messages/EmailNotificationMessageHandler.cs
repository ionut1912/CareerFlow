using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Constants;
using CareerFlow.Core.Rabbit.Events.Events;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.Messages;

public partial class EmailNotificationMessageHandler
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationMessageHandler> _logger;

    public EmailNotificationMessageHandler(IEmailService emailService, ILogger<EmailNotificationMessageHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(emailService);
        ArgumentNullException.ThrowIfNull(logger);
        _emailService = emailService;
        _logger = logger;
    }

    [UsedImplicitly]
    public async Task Handle(ResetPasswordNotificationMessage message, CancellationToken cancellationToken)
    {
        var placeholders = new Dictionary<string, string>
        {
            { "NumeAplicatie", "CareerFlow" },
            { "Nume", message.Name },
            { "LinkResetare", message.ResetLink },
            { "AnCurent", DateTime.UtcNow.Year.ToString(System.Globalization.CultureInfo.InvariantCulture) }
        };

        bool result = await _emailService.SendEmailWithTemplateAsync(message.Email, EmailConstants.ResetPasswordTemplateId, placeholders);

        if (result)
            LogResetPasswordEmailSent();
        else
            LogResetPasswordEmailFailed();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Reset password email was sent to required email")]
    private partial void LogResetPasswordEmailSent();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Reset password email was not sent to required email")]
    private partial void LogResetPasswordEmailFailed();
}
