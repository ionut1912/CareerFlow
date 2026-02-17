using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using PostmarkDotNet;

namespace CareerFlow.Core.Infrastructure.Gateways;

public class PostmarkMailClient : IMailClient
{
    private readonly PostmarkClient _client;
    private readonly PostmarkSettings _settings;

    public PostmarkMailClient(IOptions<PostmarkSettings> options)
    {
        _settings = options.Value;
        _client = new PostmarkClient(_settings.ServerToken);
    }

    public async Task<bool> SendTemplatedEmailAsync(string to, int templateId, Dictionary<string, string> model, CancellationToken cancellationToken)
    {
        var message = new TemplatedPostmarkMessage
        {
            To = to,
            From = _settings.FromAddress,
            TemplateId = templateId,
            TemplateModel = model,
            TrackOpens = true
        };

        var result = await _client.SendMessageAsync(message);

        return result.Status == PostmarkStatus.Success;
    }
}