using System.Net.Http.Headers;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Infrastructure.Persistance;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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

    public HttpClient CreateAuthenticatedClient(Account account)
    {
        var client = CreateClient();

        // You need to access your TokenService or whoever handles JWT generation
        using var scope = Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        // Generate a token for the non-existent ID
        var token = tokenService.GenerateToken(account);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
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