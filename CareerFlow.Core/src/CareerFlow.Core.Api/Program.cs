using CareerFlow.Core.Api.Endpoints;
using CareerFlow.Core.Api.Mappers;
using CareerFlow.Core.Application.Messages;
using CareerFlow.Core.Application.Validators;
using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Extensions;
using CareerFlow.Core.Infrastructure.Gateways;
using CareerFlow.Core.Infrastructure.HangfireJobs;
using CareerFlow.Core.Infrastructure.Persistance;
using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Services;
using CareerFlow.Core.Rabbit.Events.Events;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using InfisicalConfiguration;
using Microsoft.AspNetCore.HttpOverrides;
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
var env = builder.Environment.IsProduction() ? "prod" : "dev";

if (!string.IsNullOrWhiteSpace(infisicalClientId) &&
    !string.IsNullOrWhiteSpace(infisicalProjectId) &&
    !string.IsNullOrWhiteSpace(infisicalClientSecret))
    builder.Configuration.AddInfisical(new InfisicalConfigBuilder()
        .SetProjectId(infisicalProjectId)
        .SetEnvironment(env)
        .SetAuth(new InfisicalAuthBuilder()
            .SetUniversalAuth(infisicalClientId, infisicalClientSecret)
            .Build())
        .Build());

builder.AddWolverineMessaging(
    typeof(EmailNotificationMessageHandler).Assembly,
    (appBuilder, opt) =>
    {
        var emailQueueName = "email-notifications-queue";
        opt.PublishMessage<ResetPasswordNotificationMessage>().ToRabbitQueue(emailQueueName);
        opt.ListenToRabbitQueue(emailQueueName).UseDurableInbox();
    });

builder.Services.Configure<SocialAuthSettings>(builder.Configuration.GetSection(SocialAuthSettings.SectionName));
builder.Services.Configure<PostmarkSettings>(builder.Configuration.GetSection(PostmarkSettings.SectionName));
builder.Services.Configure<LegalDocSettings>(builder.Configuration.GetSection(LegalDocSettings.SectionName));

builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddHttpClient<IGithubPagesRequestsSender, GithubPagesRequestsSender>();

builder.Services.AddScoped<LegalDocumentCheckerJob>();
builder.Services.AddOpenAIIntegration();
builder.Services
    .AddDatabaseConfig<ApplicationDbContext>(builder.Configuration)
    .AddRepository<Account, AccountRepository, IAccountRepository, ApplicationDbContext>()
    .AddRepository<RefreshToken, RefreshTokenRepository, IRefreshTokenRepository, ApplicationDbContext>()
    .AddRepositoriesConfig<ITokenService, TokenService>()
    .AddRepositoriesConfig<IPasswordService, PasswordService>()
    .AddRepositoriesConfig<IAuthService, AuthService>()
    .AddRepositoriesConfig<IUnitOfWork, UnitOfWork>()
    .AddRepositoriesConfig<IEmailService, EmailService>()
    .AddRepositoriesConfig<IGoogleTokenValidator, GoogleTokenValidator>()
    .AddRepositoriesConfig<IMailClient, PostmarkMailClient>()
    .AddRepositoriesConfig<ISocialService, SocialService>()
    .AddRepositoriesConfig<ILegalService, LegalService>()
    .AddAplicationConfig(typeof(ValidationsAssemblyReference).Assembly)
    .AddPresentation<ExceptionMapper>(builder.Configuration, "CareerFlowCore");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HangfirePolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

var app = builder.Build();

app.MigrateDatabaseConfig<ApplicationDbContext>();

app.UseGlobalExceptionHandler<CareerFlow.Core.Api.Program>()
    .UseRequestDurationLogging<CareerFlow.Core.Api.Program>()
    .UseStandardMiddleware()
    .MapStandardEndpoints();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.MapApiDocumentation();
app.MapEndpoints(typeof(AccountEndpointGroup).Assembly);
app.MapClientEndpoints();

app.Logger.LogInformation("🚀 {ServiceName} starting up in {Environment} environment", "CareerFlowCore", env);

app.MapHangfireDashboard("/hangfire", new DashboardOptions
        { Authorization = new List<IDashboardAuthorizationFilter>() })
    .RequireAuthorization("HangfirePolicy");

RecurringJob.AddOrUpdate<LegalDocumentCheckerJob>(
    "check-terms-update",
    job => job.CheckForUpdatesAsync("Terms", CancellationToken.None),
    Cron.Hourly);

RecurringJob.AddOrUpdate<LegalDocumentCheckerJob>(
    "check-privacy-update",
    job => job.CheckForUpdatesAsync("Privacy", CancellationToken.None),
    Cron.Hourly);
app.Run();

namespace CareerFlow.Core.Api
{
    public class Program
    {
    }
}