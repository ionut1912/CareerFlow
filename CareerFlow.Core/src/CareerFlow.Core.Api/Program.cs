using CareerFlow.Core.Api.Endpoints;
using CareerFlow.Core.Api.Mappers;
using CareerFlow.Core.Application.Messages;
using CareerFlow.Core.Application.Validators;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Rabbit.Events.Events;
using CarrerFlow.Core.Infrastructure.Configurations;
using CarrerFlow.Core.Infrastructure.Persistance;
using CarrerFlow.Core.Infrastructure.Persistance.Repositories;
using CarrerFlow.Core.Infrastructure.Services;
using InfisicalConfiguration;
using Shared.Api.Extensions;
using Shared.Api.Infrastructure;
using Shared.Domain.Interfaces;
using Shared.Infra.Services;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var infisicalClientId = configuration["Infisical:ClientId"];
var infisicalClientSecret = configuration["Infisical:ClientSecret"];
var infisicalProjectId = configuration["Infisical:ProjectId"];

if (!string.IsNullOrWhiteSpace(infisicalClientId)&&!string.IsNullOrWhiteSpace(infisicalProjectId)&&!string.IsNullOrWhiteSpace(infisicalClientSecret))
{
    builder.Configuration.AddInfisical(new InfisicalConfigBuilder()
        .SetProjectId(infisicalProjectId)
        .SetEnvironment("dev") // or "prod", based on logic
        .SetAuth(new InfisicalAuthBuilder()
            .SetUniversalAuth(infisicalClientId, infisicalClientSecret)
            .Build())
        .Build());
}

var otelEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://tempo:4317";
var serviceName = configuration["OTEL_SERVICE_NAME"] ?? "CarrerFlowCore";
var environmentName = builder.Environment.EnvironmentName ?? "Development";

// Use the correct Loki OTLP endpoint
var lokiEndpoint = configuration["OTEL_EXPORTER_OTLP_LOGS_ENDPOINT"] ?? "http://loki:3100";

var resourceBuilder = OpenTelemetryExtensions.CreateServiceResourceBuilder(serviceName, environmentName);

builder.AddWolverineMessaging(
    typeof(EmailNotificationMessageHandler).Assembly,
    (appBuilder, opt) =>
    {

        opt.ListenToRabbitQueue("email-notifications").UseDurableInbox();

        opt.PublishMessage<EmailNotificationMessage>().ToRabbitQueue("email-notifications");
    });

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "CarrerFlow_";
});

builder.AddOpenTelemetry(lokiEndpoint, resourceBuilder);
builder.Services.Configure<SocialAuthSettings>(
    builder.Configuration.GetSection("Authentication"));

builder.Services.AddHttpClient<IAuthService, AuthService>();

builder.Services
    .AddDatabaseConfig<ApplicationDbContext>(builder.Configuration)
    .AddRepository<Account, AccountRepository, IAccountRepository, ApplicationDbContext>()
    .AddRepository<LegalDoc, LegalDocRepository, ILegalDocRepository, ApplicationDbContext>()
    .AddRepository<RefreshToken, RefreshTokenRepository, IRefreshTokenRepository, ApplicationDbContext>()
    .AddRepositoriesConfig<IJwtTokenService, JwtTokenService>()
    .AddRepositoriesConfig<IPasswordService, PasswordService>()
    .AddRepositoriesConfig<IAuthService, AuthService>()
    .AddRepositoriesConfig<IUnitOfWork, UnitOfWork>()
    .AddRepositoriesConfig<ICacheService, CacheService>()
    .AddAplicationConfig(typeof(ValidationsAssemblyReference).Assembly)
    .AddPresentation<ExceptionMapper>(builder.Configuration, otelEndpoint, serviceName, environmentName);

var app = builder.Build();

app.MigrateDatabaseConfig<ApplicationDbContext>();

app.UseGlobalExceptionHandler<Program>()
    .UseRequestDurationLogging<Program>()
    .UseStandardMiddleware()
    .MapStandardEndpoints();

app.MapApiDocumentation();
app.MapEndpoints(typeof(AccountEndpointGroup).Assembly);

app.Logger.LogInformation("🚀 {ServiceName} starting up in {Environment} environment", serviceName, environmentName);

app.Run();