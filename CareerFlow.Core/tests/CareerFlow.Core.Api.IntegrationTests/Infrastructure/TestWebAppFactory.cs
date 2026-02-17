using CareerFlow.Core.Infrastructure.Persistance;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shared.Application.Options;
using Wolverine;
using Wolverine.Persistence.Durability;

namespace CareerFlow.Core.Api.IntegrationTests.Infrastructure;

public sealed class TestWebAppFactory : WebApplicationFactory<Program>
{
    private string _postgresConnectionString = string.Empty;
    private string _redisConnectionString = string.Empty;
    private string _rabbitHost = string.Empty;
    private string _rabbitUser = string.Empty;
    private string _rabbitPass = string.Empty;
    private int _rabbitPort = 5672;

    public TestWebAppFactory WithPostgres(string connectionString)
    {
        _postgresConnectionString = connectionString;
        return this;
    }

    public TestWebAppFactory WithRedis(string connectionString)
    {
        _redisConnectionString = connectionString;
        return this;
    }

    public TestWebAppFactory WithRabbitMq(string host, string username, string password, int port = 5672)
    {
        _rabbitHost = host;
        _rabbitUser = username;
        _rabbitPass = password;
        _rabbitPort = port;
        return this;
    }

    public HttpClient CreateAnonymousClient() =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });

    public HttpClient CreateAuthenticatedClient(Guid accountId)
    {
        Server.Services.GetRequiredService<TestUserContext>().SetAccountId(accountId);
        return CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = false
        });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgresConnectionString,
                ["ConnectionStrings:Redis"] = _redisConnectionString,
                [$"{RabbitMqOptions.SectionName}:Host"] = _rabbitHost,
                [$"{RabbitMqOptions.SectionName}:Username"] = _rabbitUser,
                [$"{RabbitMqOptions.SectionName}:Password"] = _rabbitPass,
                [$"{RabbitMqOptions.SectionName}:Port"] = _rabbitPort.ToString(),
                ["RunMigrationsOnStartup"] = "false",
                ["JwtSettings:Key"] = "test-only-key-not-used-in-validation-32chars!",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
            });
        });

        builder.ConfigureServices(services =>
        {
            if (!string.IsNullOrWhiteSpace(_postgresConnectionString))
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.AddDbContext<ApplicationDbContext>(opts =>
                    opts.UseNpgsql(_postgresConnectionString));
            }

            // Mutable singleton — shared across anon and authed clients on same server.
            services.RemoveAll<TestUserContext>();
            services.AddSingleton(new TestUserContext(Guid.Empty));

            // Register the test scheme handler.
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestBearerHandler>(
                    TestBearerHandler.SchemeName, _ => { });

            // Override AuthenticationOptions AFTER all other registrations so
            // TestBearerHandler wins as the default/challenge/forbid scheme,
            // regardless of what JWT or AddPresentation registered before.
            services.PostConfigureAll<AuthenticationOptions>(opts =>
            {
                opts.DefaultAuthenticateScheme = TestBearerHandler.SchemeName;
                opts.DefaultChallengeScheme = TestBearerHandler.SchemeName;
                opts.DefaultForbidScheme = TestBearerHandler.SchemeName;
                opts.DefaultSignInScheme = TestBearerHandler.SchemeName;
                opts.DefaultSignOutScheme = TestBearerHandler.SchemeName;
                opts.DefaultScheme = TestBearerHandler.SchemeName;
            });

            services.AddExceptionHandler<DiagnosticExceptionHandler>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        if (!string.IsNullOrWhiteSpace(_postgresConnectionString))
        {
            using var scope = host.Services.CreateScope();
            var sp = scope.ServiceProvider;

            var db = sp.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();

            var store = sp.GetService<IMessageStore>();
            store?.Admin.RebuildAsync().GetAwaiter().GetResult();
        }

        return host;
    }

    private sealed class DiagnosticExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "text/plain";
            await httpContext.Response.WriteAsync(
                $"[TEST 500] {exception.GetType().FullName}: {exception.Message}\n\n{exception}",
                cancellationToken);
            return true;
        }
    }
}