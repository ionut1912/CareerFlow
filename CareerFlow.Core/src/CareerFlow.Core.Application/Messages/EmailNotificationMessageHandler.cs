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

    public async Task Handle(ResetPasswordNotificationMessage message)
    {

        var placeholders = new Dictionary<string, string>
        {
            {"NumeAplicatie","CareerFlow" },
            { "Nume", message.Nume },
            { "LinkResetare", message.ResetLink },
            {"AnCurent",DateTime.UtcNow.Year.ToString() }
        };

        await _emailService.SendEmailWithTemplateAsync(message.Email,  "Password reset", placeholders);
        _logger.LogInformation("Email de resetare a parolei trimis către {Email}", message.Email);
    }
}
