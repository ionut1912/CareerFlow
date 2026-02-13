using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostmarkDotNet;

namespace CareerFlow.Core.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly PostmarkSettings _settings;
    private readonly ILogger<EmailService> _logger;
    private readonly PostmarkClient _client;

    public EmailService(IOptions<PostmarkSettings> options, ILogger<EmailService> logger)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _settings = options.Value;
        _logger = logger;
        _client = new PostmarkClient(_settings.ServerToken);
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
            _logger.LogError("Receiver email addres is empty");
        }

        var message = new TemplatedPostmarkMessage
        {
            To = to,
            From = _settings.FromAddress,
            TemplateId = templateId,
            TemplateModel = templateModel,
            TrackOpens = true
        };

        try
        {
            var result = await _client.SendMessageAsync(message);

            if (result.Status == PostmarkStatus.Success)
            {
                _logger.LogInformation("Email sent successfully  using template {TemplateId}", templateId);
                return true;
            }

            _logger.LogError("Failed to send email.Postmark Error: {Message}", result.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email with template {TemplateId}", templateId);
            return false;
        }
    }
}