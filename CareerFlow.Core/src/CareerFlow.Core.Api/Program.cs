using CareerFlow.Core.Api.Features.Account;
using CareerFlow.Core.Api.Mappers;
using CareerFlow.Core.Application.Messages;
using CareerFlow.Core.Application.Serialization;
using CareerFlow.Core.Application.Validators;
using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Configurations;
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
using Shared.Application.Extensions;
using Shared.Domain.Interfaces;
using Shared.Infra.Extensions;
using Shared.Infra.Services;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, CareerFlowJsonContext.Default);
});

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

builder.Services.AddMemoryCache();
builder.Services.AddScoped<LegalDocumentCheckerJob>();
builder.Services
    .AddDatabase<ApplicationDbContext>(builder.Configuration)
    .AddRepository<Account, AccountRepository, IAccountRepository, ApplicationDbContext>()
    .AddRepository<RefreshToken, RefreshTokenRepository, IRefreshTokenRepository, ApplicationDbContext>()
    .AddRepository<UserProfile, UserProfileRepository, IUserProfileRepository, ApplicationDbContext>()
    .AddRepos<ITokenService, TokenService>()
    .AddRepos<IPasswordService, PasswordService>()
    .AddRepos<IAuthService, AuthService>()
    .AddRepos<IUnitOfWork, UnitOfWork>()
    .AddRepos<IEmailService, EmailService>()
    .AddRepos<IGoogleTokenValidator, GoogleTokenValidator>()
    .AddRepos<IMailClient, PostmarkMailClient>()
    .AddRepos<ISocialService, SocialService>()
    .AddRepos<ILegalService, LegalService>()
    .AddApplicationServices(typeof(ValidationsAssemblyReference).Assembly)
    .AddPresentation<ExceptionMapper>(builder.Configuration, "CareerFlowCore");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HangfirePolicy", policy => { policy.RequireAuthenticatedUser(); });
});

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options =>
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

var app = builder.Build();

app.MigrateServiceDatabase<ApplicationDbContext>();

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
    Cron.Daily);

RecurringJob.AddOrUpdate<LegalDocumentCheckerJob>(
    "check-privacy-update",
    job => job.CheckForUpdatesAsync("Privacy", CancellationToken.None),
    Cron.Daily);
app.Run();

namespace CareerFlow.Core.Api
{
    public class Program
    {
    }
}