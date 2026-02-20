using CareerFlow.Core.Api.Endpoints;
using CareerFlow.Core.Api.Mappers;
using CareerFlow.Core.Application.Messages;
using CareerFlow.Core.Application.Validators;
using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Repositories;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Exceptions;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Gateways;
using CareerFlow.Core.Infrastructure.Persistance;
using CareerFlow.Core.Infrastructure.Persistance.Repositories;
using CareerFlow.Core.Infrastructure.Services;
using CareerFlow.Core.Rabbit.Events.Events;
using FluentValidation;
using InfisicalConfiguration;
using Shared.Api.Extensions;
using Shared.Api.Infrastructure;
using Shared.Domain.Exceptions;
using Shared.Domain.Interfaces;
using Shared.Infra.Services;
using Wolverine.ErrorHandling;
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

        opt.Policies.OnException<ValidationException>().Discard();
        opt.Policies.OnException<AccountNotFoundException>().Discard();
        opt.Policies.OnException<InvalidFieldException>().Discard();
        opt.Policies.OnException<PasswordNotMatchException>().Discard();
        opt.Policies.OnException<UserAlreadyExistsException>().Discard();
        opt.Policies.OnException<InvalidRefreshTokenException>().Discard();
        opt.Policies.OnException<TokenAlreadyUsedExcception>().Discard();
        opt.Policies.OnException<TokenRevokedException>().Discard();
        opt.Policies.OnException<CustomValidationException>().Discard();
    });

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "CareerFlow_";
});

builder.Services.Configure<SocialAuthSettings>(
    builder.Configuration.GetSection(SocialAuthSettings.SectionName));

builder.Services.Configure<PostmarkSettings>(
    builder.Configuration.GetSection(PostmarkSettings.SectionName));

builder.Services.AddHttpClient<IAuthService, AuthService>();

builder.Services
    .AddDatabaseConfig<ApplicationDbContext>(builder.Configuration)
    .AddRepository<Account, AccountRepository, IAccountRepository, ApplicationDbContext>()
    .AddRepository<RefreshToken, RefreshTokenRepository, IRefreshTokenRepository, ApplicationDbContext>()
    .AddRepositoriesConfig<ITokenService, TokenService>()
    .AddRepositoriesConfig<IPasswordService, PasswordService>()
    .AddRepositoriesConfig<IAuthService, AuthService>()
    .AddRepositoriesConfig<IUnitOfWork, UnitOfWork>()
    .AddRepositoriesConfig<ICacheService, CacheService>()
    .AddRepositoriesConfig<IEmailService, EmailService>()
    .AddRepositoriesConfig<IGoogleTokenValidator, GoogleTokenValidator>()
    .AddRepositoriesConfig<IMailClient, PostmarkMailClient>()
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

public partial class Program
{
}