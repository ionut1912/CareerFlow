using System.Net.Http.Headers;
using CareerFlow.Core.Infrastructure.Persistance;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests.Setup;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine").Build();

    private readonly RabbitMqContainer _rabbitContainer = new RabbitMqBuilder("rabbitmq:3-management-alpine")
        .WithPortBinding(15672, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Server startup complete.*"))
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:alpine").Build();

    public string DbConnectionString => _dbContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _dbContainer.StartAsync(),
            _redisContainer.StartAsync(),
            _rabbitContainer.StartAsync()
        );

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        await Task.WhenAll(
            _dbContainer.StopAsync(),
            _redisContainer.StopAsync(),
            _rabbitContainer.StopAsync()
        );
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings__DefaultConnection"] = DbConnectionString,
                ["ConnectionStrings__Redis"] = _redisContainer.GetConnectionString(),
                ["RabbitMQ__Host"] = _rabbitContainer.Hostname,
                ["RabbitMQ__Port"] = _rabbitContainer.GetMappedPublicPort(5672).ToString(),
                ["RabbitMQ__Username"] = "rabbitmq",
                ["RabbitMQ__Password"] = "rabbitmq",
                ["JwtSettings__Key"] = "testjwtsuperlongkeyforauthentication",
                ["JwtSettings__Issuer"] = "testjwtissuer",
                ["JwtSettings__Audience"] = "testaudience"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
                options.DefaultForbidScheme = "TestScheme";
            });
        });
    }

    public HttpClient CreateAuthenticatedClient(Guid accountId)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme");
        client.DefaultRequestHeaders.Add("X-Test-AccountId", accountId.ToString());
        return client;
    }

    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(DbConnectionString);
        await conn.OpenAsync();

        try
        {
            var respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["public"]
            });

            await respawner.ResetAsync(conn);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No tables found"))
        {
        }
    }
}