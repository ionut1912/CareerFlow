using CareerFlow.Core.Api.Endpoints;
using CareerFlow.Core.Api.Mappers;
using CareerFlow.Core.Application.Messages;
using CareerFlow.Core.Application.Validators;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Persistance;
using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Services;
using CareerFlow.Core.Rabbit.Events.Events;
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
var env = builder.Environment.IsProduction() ? "prod" : "dev";
if (!string.IsNullOrWhiteSpace(infisicalClientId) &&
    !string.IsNullOrWhiteSpace(infisicalProjectId) &&
    !string.IsNullOrWhiteSpace(infisicalClientSecret))
{

    builder.Configuration.AddInfisical(new InfisicalConfigBuilder()
        .SetProjectId(infisicalProjectId)
        .SetEnvironment(env)
        .SetAuth(new InfisicalAuthBuilder()
            .SetUniversalAuth(infisicalClientId, infisicalClientSecret)
            .Build())
        .Build());
}


builder.AddWolverineMessaging(
    typeof(EmailNotificationMessageHandler).Assembly,
    (appBuilder, opt) =>
    {

        var emailQueueName = "email-notifications-queue";
        opt.PublishMessage<ResetPasswordNotificationMessage>().ToRabbitQueue(emailQueueName);
        opt.ListenToRabbitQueue(emailQueueName)
                .UseDurableInbox();
    });

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "CarrerFlow_";
});

builder.Services.Configure<SocialAuthSettings>(
    builder.Configuration.GetSection(SocialAuthSettings.SectionName));

builder.Services.Configure<PostmarkSettings>(
    builder.Configuration.GetSection(PostmarkSettings.SectionName));

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
    .AddRepositoriesConfig<IEmailService, EmailService>()
    .AddAplicationConfig(typeof(ValidationsAssemblyReference).Assembly)
    .AddPresentation<ExceptionMapper>(builder.Configuration, "CareerFlowCore");

var app = builder.Build();

app.MigrateDatabaseConfig<ApplicationDbContext>();

app.UseGlobalExceptionHandler<Program>()
    .UseRequestDurationLogging<Program>()
    .UseStandardMiddleware()
    .MapStandardEndpoints();

app.MapApiDocumentation();
app.MapEndpoints(typeof(AccountEndpointGroup).Assembly);

app.Logger.LogInformation("🚀 {ServiceName} starting up in {Environment} environment", "CareerFlowCore", env);

app.Run();