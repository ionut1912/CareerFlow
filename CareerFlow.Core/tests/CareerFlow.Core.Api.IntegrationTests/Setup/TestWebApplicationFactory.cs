using System.Net.Http.Headers;
using CareerFlow.Core.Infrastructure.Persistance;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
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
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine")
        .Build();

    private readonly RabbitMqContainer _rabbitContainer = new RabbitMqBuilder("rabbitmq:3-management-alpine")
        .WithPortBinding(15672, true)
        .WithWaitStrategy(Wait.ForUnixContainer()
            .UntilMessageIsLogged(".*Server startup complete.*"))
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:alpine")
        .Build();

    public string DbConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _dbContainer.StartAsync(),
            _redisContainer.StartAsync(),
            _rabbitContainer.StartAsync()
        );

        await Task.Delay(TimeSpan.FromSeconds(3));

        DbConnectionString = _dbContainer.GetConnectionString();

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", DbConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", _redisContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("RabbitMQ__Host", _rabbitContainer.Hostname);
        Environment.SetEnvironmentVariable("RabbitMQ__Port", _rabbitContainer.GetMappedPublicPort(5672).ToString());
        Environment.SetEnvironmentVariable("RabbitMQ__Username", "rabbitmq");
        Environment.SetEnvironmentVariable("RabbitMQ__Password", "rabbitmq");
        Environment.SetEnvironmentVariable("JwtSettings__Key", "testjwtsuperlongkeyforauthentication");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "testjwtissuer");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "testaudience");

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", null);
        Environment.SetEnvironmentVariable("RabbitMQ__Host", null);
        Environment.SetEnvironmentVariable("RabbitMQ__Port", null);
        Environment.SetEnvironmentVariable("RabbitMQ__Username", null);
        Environment.SetEnvironmentVariable("RabbitMQ__Password", null);
        Environment.SetEnvironmentVariable("JwtSettings__Key", null);
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", null);
        Environment.SetEnvironmentVariable("JwtSettings__Audience", null);

        await Task.WhenAll(
            _dbContainer.StopAsync(),
            _redisContainer.StopAsync(),
            _rabbitContainer.StopAsync()
        );
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:DefaultConnection",
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")!);
        builder.UseSetting("ConnectionStrings:Redis",
            Environment.GetEnvironmentVariable("ConnectionStrings__Redis")!);
        builder.UseSetting("RabbitMQ:Host",
            Environment.GetEnvironmentVariable("RabbitMQ__Host")!);
        builder.UseSetting("RabbitMQ:Port",
            Environment.GetEnvironmentVariable("RabbitMQ__Port")!);
        builder.UseSetting("RabbitMQ:Username",
            Environment.GetEnvironmentVariable("RabbitMQ__Username")!);
        builder.UseSetting("RabbitMQ:Password",
            Environment.GetEnvironmentVariable("RabbitMQ__Password")!);
        builder.UseSetting("JwtSettings:Key",
            Environment.GetEnvironmentVariable("JwtSettings__Key")!);
        builder.UseSetting("JwtSettings:Issuer",
            Environment.GetEnvironmentVariable("JwtSettings__Issuer")!);
        builder.UseSetting("JwtSettings:Audience",
            Environment.GetEnvironmentVariable("JwtSettings__Audience")!);

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

        var respawner = await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });

        await respawner.ResetAsync(conn);
    }
}