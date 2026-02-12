using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostmarkDotNet;

namespace CareerFlow.Core.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly PostmarkSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<PostmarkSettings> options, ILogger<EmailService> logger)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailWithTemplateAsync(string to, string templateAlias, Dictionary<string, string> templateModel)
    {
        var message = new TemplatedPostmarkMessage
        {
            To = to,
            From = _settings.FromAddress,
            TemplateAlias = templateAlias, 
            TemplateModel = templateModel,
            TrackOpens = true
        };

        try
        {
            var client = new PostmarkClient(_settings.ServerToken);
            var result = await client.SendMessageAsync(message);

            if (result.Status == PostmarkStatus.Success)
            {
                _logger.LogInformation("Email sent successfully to {To} using template {TemplateAlias}", to, templateAlias);
                return true;
            }

            _logger.LogError("Failed to send email to {To}. Postmark Error: {Message}", to, result.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {To} with template {TemplateAlias}", to, templateAlias);
            return false;
        }
    }
}