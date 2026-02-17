using Shared.Application.Options;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace CareerFlow.Core.Api.IntegrationTests.Infrastructure;

public sealed class SharedContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("careerflow_test")
        .WithUsername("cf_test")
        .WithPassword("cf_test_pass!")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public string PostgresConnectionString => _postgres.GetConnectionString();
    public string RedisConnectionString => _redis.GetConnectionString();
    public string RabbitMqConnectionString => _rabbit.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgres.StartAsync(),
            _redis.StartAsync(),
            _rabbit.StartAsync());

        var rabbitUri = new Uri(RabbitMqConnectionString);
        var userInfo = rabbitUri.UserInfo.Split(':');
        var rabbitUser = userInfo.Length > 0 ? userInfo[0] : "guest";
        var rabbitPass = userInfo.Length > 1 ? userInfo[1] : "guest";

        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", PostgresConnectionString);
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", RedisConnectionString);
        Environment.SetEnvironmentVariable($"{RabbitMqOptions.SectionName}__Host", rabbitUri.Host);
        Environment.SetEnvironmentVariable($"{RabbitMqOptions.SectionName}__Username", rabbitUser);
        Environment.SetEnvironmentVariable($"{RabbitMqOptions.SectionName}__Password", rabbitPass);
        Environment.SetEnvironmentVariable($"{RabbitMqOptions.SectionName}__Port", rabbitUri.Port.ToString());
    }

    public async Task DisposeAsync()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__Redis", null);
        Environment.SetEnvironmentVariable($"{RabbitMqOptions.SectionName}__Host", null);
        Environment.SetEnvironmentVariable($"{RabbitMqOptions.SectionName}__Username", null);
        Environment.SetEnvironmentVariable($"{RabbitMqOptions.SectionName}__Password", null);
        Environment.SetEnvironmentVariable($"{RabbitMqOptions.SectionName}__Port", null);

        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _redis.DisposeAsync().AsTask(),
            _rabbit.DisposeAsync().AsTask());
    }
}