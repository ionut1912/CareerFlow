using CareerFlow.Core.Infrastructure.Services;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using Shouldly;
using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Test.Integration;

[Trait("Category", "Integration")]
public class CacheServiceIntegrationTests : IAsyncLifetime
{
    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine")
        .Build();

    private IConnectionMultiplexer _multiplexer = null!;
    private CacheService _sut = null!;

    public async Task InitializeAsync()
    {
        await _redis.StartAsync();

        _multiplexer = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString() + ",allowAdmin=true");

        var redisCacheOptions = new RedisCacheOptions
        {
            ConnectionMultiplexerFactory = () => Task.FromResult(_multiplexer)
        };

        var distributedCache = new RedisCache(Options.Create(redisCacheOptions));
        _sut = new CacheService(distributedCache);
    }

    public async Task DisposeAsync()
    {
        _multiplexer?.Dispose();
        await _redis.DisposeAsync();
    }

    private async Task FlushAsync()
    {
        var server = _multiplexer.GetServer(_redis.GetConnectionString());
        await server.FlushAllDatabasesAsync();
    }

    [Fact]
    public async Task GetCacheValueAsync_KeyNotSet_ReturnsNull()
    {
        // Arrange
        await FlushAsync();

        // Act
        var result = await _sut.GetCacheValueAsync<string>("never-set");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetCacheValueAsync_IntKeyNotSet_ReturnsZero()
    {
        // Arrange
        await FlushAsync();

        // Act
        var result = await _sut.GetCacheValueAsync<int>("missing-int");

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task GetCacheValueAsync_AfterSetString_ReturnsCorrectValue()
    {
        // Arrange
        await FlushAsync();
        await _sut.SetCacheValueAsync("greet", "Hello, Redis!");

        // Act
        var result = await _sut.GetCacheValueAsync<string>("greet");

        // Assert
        result.ShouldBe("Hello, Redis!");
    }

    [Fact]
    public async Task GetCacheValueAsync_AfterSetInt_ReturnsCorrectValue()
    {
        // Arrange
        await FlushAsync();
        await _sut.SetCacheValueAsync("count", 42);

        // Act
        var result = await _sut.GetCacheValueAsync<int>("count");

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public async Task GetCacheValueAsync_AfterSetComplexObject_ReturnsDeserializedObject()
    {
        // Arrange
        await FlushAsync();
        var payload = new CachePayload { UserId = Guid.NewGuid(), Role = "Admin" };
        await _sut.SetCacheValueAsync("payload", payload);

        // Act
        var result = await _sut.GetCacheValueAsync<CachePayload>("payload");

        // Assert
        result.ShouldNotBeNull();
        result.UserId.ShouldBe(payload.UserId);
        result.Role.ShouldBe("Admin");
    }

    [Fact]
    public async Task GetCacheValueAsync_AfterStoringNull_ReturnsNull()
    {
        // Arrange
        await FlushAsync();
        await _sut.SetCacheValueAsync<string?>("nullable", null);

        // Act
        var result = await _sut.GetCacheValueAsync<string?>("nullable");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task SetCacheValueAsync_OverwriteExistingKey_ReturnsLatestValue()
    {
        // Arrange
        await FlushAsync();
        await _sut.SetCacheValueAsync("overwrite", "first");

        // Act
        await _sut.SetCacheValueAsync("overwrite", "second");
        var result = await _sut.GetCacheValueAsync<string>("overwrite");

        // Assert
        result.ShouldBe("second");
    }

    [Fact]
    public async Task SetCacheValueAsync_MultipleKeys_StoredIndependently()
    {
        // Arrange
        await FlushAsync();
        await _sut.SetCacheValueAsync("key-a", "value-a");
        await _sut.SetCacheValueAsync("key-b", "value-b");

        // Act
        var a = await _sut.GetCacheValueAsync<string>("key-a");
        var b = await _sut.GetCacheValueAsync<string>("key-b");

        // Assert
        a.ShouldBe("value-a");
        b.ShouldBe("value-b");
    }

    [Fact]
    public async Task GetCacheValueAsync_AfterFlush_ReturnsNull()
    {
        // Arrange
        await _sut.SetCacheValueAsync("will-be-flushed", "data");
        await FlushAsync();

        // Act
        var result = await _sut.GetCacheValueAsync<string>("will-be-flushed");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task SetCacheValueAsync_ListValue_RoundTripSucceeds()
    {
        // Arrange
        await FlushAsync();
        var list = new List<string> { "a", "b", "c" };
        await _sut.SetCacheValueAsync("my-list", list);

        // Act
        var result = await _sut.GetCacheValueAsync<List<string>>("my-list");

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
        result.ShouldContain("b");
    }

    private sealed record CachePayload
    {
        public Guid UserId { get; init; }
        public string Role { get; init; } = string.Empty;
    }
}