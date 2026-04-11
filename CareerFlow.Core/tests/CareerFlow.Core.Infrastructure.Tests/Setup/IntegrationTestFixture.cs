using Testcontainers.PostgreSql;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Setup;

public class IntegrationTestFixture : IAsyncLifetime
{
    // FIX: Pass the image directly into the constructor
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("careerflow_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition("RepositoryCollection")]
public class RepositoryCollection : ICollectionFixture<IntegrationTestFixture>
{
}