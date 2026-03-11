using System.Net.Http.Headers;
using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Http;
using CareerFlow.Core.Infrastructure.OpenAIAbstractions;
using CareerFlow.Core.Infrastructure.Services.OpenAi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareerFlow.Core.Infrastructure.Extensions;

public static class OpenAIServiceExtensions
{
    public static IServiceCollection AddOpenAIIntegration(
        this IServiceCollection services)
    {
        // Options
        services.AddOptions<OpenAIOptions>()
            .BindConfiguration(OpenAIOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // HTTP client with Polly resilience
        services.AddHttpClient<IOpenAIHttpClient, OpenAIHttpClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<OpenAIOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", options.ApiKey);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .AddStandardResilienceHandler();

        // Completion: core impl
        services.AddScoped<OpenAICompletionService>();

        // Completion: decorated with logging
        services.AddScoped<IAICompletionService>(sp =>
            new LoggingCompletionService(
                sp.GetRequiredService<OpenAICompletionService>(),
                sp.GetRequiredService<ILogger<LoggingCompletionService>>()));

        // Embedding service (no decorator needed — straightforward)
        services.AddScoped<IAIEmbeddingService, OpenAIEmbeddingService>();
        services.AddScoped<ChatApplicationService>();
        return services;
    }
}