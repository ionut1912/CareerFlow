using System.Globalization;
using System.Net.Http.Headers;

using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Domain.Entities;
using CareerFlow.Core.Domain.Models.Authentication;
using CareerFlow.Core.Infrastructure.Persistence;

using DotNet.Testcontainers.Builders;

using JetBrains.Annotations;

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

namespace CareerFlow.Core.Api.Tests.Setup;

[UsedImplicitly]
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:15-alpine").Build();

    private readonly RabbitMqContainer _rabbitContainer = new RabbitMqBuilder("rabbitmq:3-management-alpine")
        .WithPortBinding(15672, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged(".*Server startup complete.*"))
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder("redis:alpine").Build();

    private string DbConnectionString => _dbContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _dbContainer.StartAsync(),
            _redisContainer.StartAsync(),
            _rabbitContainer.StartAsync()
        );

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", DbConnectionString);
        Environment.SetEnvironmentVariable("Redis__ConnectionString", _redisContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("RabbitMQ__Host", _rabbitContainer.Hostname);
        Environment.SetEnvironmentVariable("RabbitMQ__Port",
            _rabbitContainer.GetMappedPublicPort(5672).ToString(CultureInfo.InvariantCulture));
        Environment.SetEnvironmentVariable("RabbitMQ__Username", "rabbitmq");
        Environment.SetEnvironmentVariable("RabbitMQ__Password", "rabbitmq");
        Environment.SetEnvironmentVariable("JwtSettings__Key", "testjwtsuperlongkeyforauthentication");
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", "testjwtissuer");
        Environment.SetEnvironmentVariable("JwtSettings__Audience", "testaudience");
        Environment.SetEnvironmentVariable("OpenAI__ApiKey", "testkey");
        Environment.SetEnvironmentVariable("R2__AccountId", "test-account-id");
        Environment.SetEnvironmentVariable("R2__AccessKey", "test-access-key");
        Environment.SetEnvironmentVariable("R2__SecretKey", "test-secret-key");
        Environment.SetEnvironmentVariable("R2__BucketName", "careerflow-courses-test");
        Environment.SetEnvironmentVariable("Analyzer__BaseUrl", "http://localhost:8080/ai");
        Environment.SetEnvironmentVariable("Authentication__BaseUrl", "https://localhost");
        Environment.SetEnvironmentVariable("Authentication__Google__ClientId", "test-google-client-id");
        Environment.SetEnvironmentVariable("Authentication__Google__ClientSecret", "test-google-client-secret");
        Environment.SetEnvironmentVariable("Authentication__LinkedIn__ClientId", "test-linkedin-client-id");
        Environment.SetEnvironmentVariable("Authentication__LinkedIn__ClientSecret", "test-linkedin-client-secret");
        Environment.SetEnvironmentVariable("Authentication__LinkedIn__RedirectUri",
            "https://localhost/social/auth/linkedin/mobile/callback");

        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        Environment.SetEnvironmentVariable("Redis__ConnectionString", null);
        Environment.SetEnvironmentVariable("RabbitMQ__Host", null);
        Environment.SetEnvironmentVariable("RabbitMQ__Port", null);
        Environment.SetEnvironmentVariable("RabbitMQ__Username", null);
        Environment.SetEnvironmentVariable("RabbitMQ__Password", null);
        Environment.SetEnvironmentVariable("JwtSettings__Key", null);
        Environment.SetEnvironmentVariable("JwtSettings__Issuer", null);
        Environment.SetEnvironmentVariable("JwtSettings__Audience", null);
        Environment.SetEnvironmentVariable("OpenAI__ApiKey", null);
        Environment.SetEnvironmentVariable("R2__AccountId", null);
        Environment.SetEnvironmentVariable("R2__AccessKey", null);
        Environment.SetEnvironmentVariable("R2__SecretKey", null);
        Environment.SetEnvironmentVariable("R2__BucketName", null);
        Environment.SetEnvironmentVariable("Analyzer__BaseUrl", null);
        Environment.SetEnvironmentVariable("Authentication__BaseUrl", null);
        Environment.SetEnvironmentVariable("Authentication__Google__ClientId", null);
        Environment.SetEnvironmentVariable("Authentication__Google__ClientSecret", null);
        Environment.SetEnvironmentVariable("Authentication__LinkedIn__ClientId", null);
        Environment.SetEnvironmentVariable("Authentication__LinkedIn__ClientSecret", null);
        Environment.SetEnvironmentVariable("Authentication__LinkedIn__RedirectUri", null);

        await Task.WhenAll(
            _dbContainer.StopAsync(),
            _redisContainer.StopAsync(),
            _rabbitContainer.StopAsync()
        );
    }

    public HttpClient CreateAuthenticatedClient(Account account)
    {
        HttpClient client = CreateClient();
        using IServiceScope scope = Services.CreateScope();
        ITokenService tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        AuthResult token = tokenService.GenerateToken(account);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Testing");

    public async Task ResetDatabaseAsync()
    {
        await using var conn = new NpgsqlConnection(DbConnectionString);
        await conn.OpenAsync();

        try
        {
            Respawner respawner = await Respawner.CreateAsync(conn,
                new RespawnerOptions { DbAdapter = DbAdapter.Postgres, SchemasToInclude = ["public"] });

            await respawner.ResetAsync(conn);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No tables found"))
        {
        }
    }
}
