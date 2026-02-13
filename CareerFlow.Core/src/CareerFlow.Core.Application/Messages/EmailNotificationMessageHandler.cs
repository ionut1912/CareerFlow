using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Rabbit.Events.Events;
using Microsoft.Extensions.Logging;

namespace CareerFlow.Core.Application.Messages;

public class EmailNotificationMessageHandler
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailNotificationMessageHandler> _logger;


    public EmailNotificationMessageHandler(IEmailService emailService, ILogger<EmailNotificationMessageHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(emailService, nameof(emailService));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Handle(ResetPasswordNotificationMessage message, CancellationToken cancellationToken)
    {

        var placeholders = new Dictionary<string, string>
        {
            {"NumeAplicatie","CareerFlow" },
            { "Nume", message.Name },
            { "LinkResetare", message.ResetLink },
            {"AnCurent",DateTime.UtcNow.Year.ToString() }
        };

        var result = await _emailService.SendEmailWithTemplateAsync(message.Email, 43498403, placeholders, cancellationToken);
        if (result == true)
        {
            _logger.LogInformation("Reset password email was sent to required email");
        }
        else
        {
            _logger.LogWarning("Reset password email was not sent to required email");
        }
    }
}
