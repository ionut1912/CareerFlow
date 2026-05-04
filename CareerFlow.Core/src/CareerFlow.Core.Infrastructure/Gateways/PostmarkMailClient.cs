using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Infrastructure.Configurations;

using JetBrains.Annotations;

using Microsoft.Extensions.Options;

using PostmarkDotNet;

namespace CareerFlow.Core.Infrastructure.Gateways;

[UsedImplicitly]
public class PostmarkMailClient : IMailClient
{
    private readonly PostmarkClient _client;
    private readonly PostmarkSettings _settings;

    public PostmarkMailClient(IOptions<PostmarkSettings> options)
    {
        _settings = options.Value;
        _client = new PostmarkClient(_settings.ServerToken);
    }

    public async Task<bool> SendTemplatedEmailAsync(string receiver, int templateId, Dictionary<string, string> model)
    {
        var message = new TemplatedPostmarkMessage
        {
            To = receiver,
            From = _settings.FromAddress,
            TemplateId = templateId,
            TemplateModel = model,
            TrackOpens = true
        };

        PostmarkResponse? result = await _client.SendMessageAsync(message);
        return result.Status == PostmarkStatus.Success;
    }
}
