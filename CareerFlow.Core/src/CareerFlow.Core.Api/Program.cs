
using CareerFlow.Core.Api.Endpoints;
using CareerFlow.Core.Api.Mappers;
using CareerFlow.Core.Application.Mediatr;
using CareerFlow.Core.Application.Validators;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Interfaces;
using CarrerFlow.Core.Infrastructure.Persistance;
using CarrerFlow.Core.Infrastructure.Persistance.Repositories;
using Shared.Api.Extensions;
using Shared.Api.Infrastructure;
using Shared.Domain.Interfaces;
using Shared.Infra.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var otelEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://tempo:4317";
var serviceName = configuration["OTEL_SERVICE_NAME"] ?? "CarrerFlowCore";
var environmentName = builder.Environment.EnvironmentName ?? "Development";

// Use the correct Loki OTLP endpoint
var lokiEndpoint = configuration["OTEL_EXPORTER_OTLP_LOGS_ENDPOINT"] ?? "http://loki:3100";

var resourceBuilder = OpenTelemetryExtensions.CreateServiceResourceBuilder(serviceName, environmentName);

builder.AddOpenTelemetry(lokiEndpoint, resourceBuilder);

builder.Services
    .AddDatabaseConfig<ApplicationDbContext>(builder.Configuration)
    .AddRepository<Account, AccountRepository, IAccountRepository, ApplicationDbContext>()
    .AddRepository<TermsAndCondition, TermAndConditionService, ITermsAndConditionsService, ApplicationDbContext>()
    .AddRepository<PrivacyPolicy, PrivacyPolicyService, IPrivacyPolicyService, ApplicationDbContext>()
    .AddRepositoriesConfig<IJwtTokenService, JwtTokenService>()
    .AddRepositoriesConfig<IPasswordService, PasswordService>()
    .AddRepositoriesConfig<IUnitOfWork, UnitOfWork>()
    .AddAplicationConfig(typeof(MediatrAssemblyReference).Assembly, typeof(ValidationsAssemblyReference).Assembly)
    .AddPresentation<ExceptionMapper>(builder.Configuration, otelEndpoint, serviceName, environmentName);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.MigrateDatabaseConfig<ApplicationDbContext>();

app.UseGlobalExceptionHandler<Program>()
    .UseRequestDurationLogging<Program>()
    .UseStandardMiddleware()
    .MapStandardEndpoints();

app.UseCors("AllowMyFrontend");

app.MapApiDocumentation();
app.MapEndpoints(typeof(UserEndpointGroup).Assembly);

app.Logger.LogInformation("🚀 {ServiceName} starting up in {Environment} environment", serviceName, environmentName);

app.Run();