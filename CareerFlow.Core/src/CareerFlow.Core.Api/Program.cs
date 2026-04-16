using CareerFlow.Core.Api.Features.Account;
using CareerFlow.Core.Api.Filters;
using CareerFlow.Core.Api.Mappers;
using CareerFlow.Core.Application.Messages;
using CareerFlow.Core.Application.Serialization;
using CareerFlow.Core.Application.Validators;
using CareerFlow.Core.Infrastructure.Extensions;
using CareerFlow.Core.Infrastructure.HangfireJobs;
using CareerFlow.Core.Infrastructure.Persistance;
using CareerFlow.Core.Rabbit.Events.Events;
using Hangfire;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Shared.Api.Extensions;
using Shared.Api.Infrastructure;
using Shared.Application.Extensions;
using Shared.Infra.Extensions;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, CareerFlowJsonContext.Default);
});

var env = builder.Environment.IsProduction() ? "prod" : "dev";
builder.Services
    .AddInfrastructure(builder.Configuration, env)
    .AddApplicationServices(typeof(ValidationsAssemblyReference).Assembly)
    .AddPresentation<ExceptionMapper>(builder.Configuration, "CareerFlowCore");

builder.Services.ConfigureAll<OpenApiOptions>(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Servers = new List<OpenApiServer>
        {
            new() { Url = "/core" }
        };
        return Task.CompletedTask;
    });
});

builder.AddWolverineMessaging(
    typeof(EmailNotificationMessageHandler).Assembly,
    (appBuilder, opt) =>
    {
        var emailQueueName = "email-notifications-queue";
        opt.PublishMessage<ResetPasswordNotificationMessage>().ToRabbitQueue(emailQueueName);
        opt.ListenToRabbitQueue(emailQueueName).UseDurableInbox();
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HangfirePolicy", policy => { policy.RequireAuthenticatedUser(); });
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UsePathBase("/core");

app.MigrateServiceDatabase<ApplicationDbContext>();
app.UseGlobalExceptionHandler<Program>()
    .UseRequestDurationLogging<Program>()
    .UseStandardMiddleware()
    .MapStandardEndpoints();

app.MapApiDocumentation();
app.MapEndpoints(typeof(AccountEndpointGroup).Assembly);
app.MapClientEndpoints();

app.Logger.LogInformation("🚀 {ServiceName} starting up in {Environment} environment", "CareerFlowCore", env);

app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthFilter() }
}).RequireAuthorization("HangfirePolicy");

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