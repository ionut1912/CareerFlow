using System.Text.Json;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using StackExchange.Redis;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Services;

public class RedisCacheServiceTests
{
    private const string InstanceName = "myapp:";
    private const int DefaultExpiryMinutes = 60;

    private readonly Mock<IConnectionMultiplexer> _connectionMock = new();
    private readonly Mock<IDatabase> _dbMock = new();
    private readonly Mock<ILogger<RedisCacheService>> _loggerMock = new();

    private readonly CacheSettings _settings = new()
    {
        InstanceName = InstanceName,
        DefaultExpiryMinutes = DefaultExpiryMinutes
    };

    private readonly RedisCacheService _sut;

    public RedisCacheServiceTests()
    {
        // The service calls GetDatabase() which resolves to GetDatabase(-1, null)
        _connectionMock
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object?>()))
            .Returns(_dbMock.Object);

        _sut = new RedisCacheService(
            _connectionMock.Object,
            Options.Create(_settings),
            _loggerMock.Object);
    }

    // Builds the prefixed key exactly as BuildKey() does internally
    private static RedisKey PrefixedKey(string raw) => $"{InstanceName}{raw}";

    // ---------------------------------------------------------------------------
    // GetAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_WhenKeyExists_ShouldReturnDeserializedValue()
    {
        const string key = "user:42";
        var expected = new SamplePayload("Alice", 30);
        var json = JsonSerializer.Serialize(expected);

        _dbMock
            .Setup(x => x.StringGetAsync(PrefixedKey(key), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)json);

        var result = await _sut.GetAsync<SamplePayload>(key);

        result.ShouldNotBeNull();
        result.Name.ShouldBe(expected.Name);
        result.Age.ShouldBe(expected.Age);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnDefault()
    {
        _dbMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        var result = await _sut.GetAsync<SamplePayload>("missing-key");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldUseKeyWithInstanceNamePrefix()
    {
        const string rawKey = "session:99";

        _dbMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        await _sut.GetAsync<SamplePayload>(rawKey);

        _dbMock.Verify(
            x => x.StringGetAsync(PrefixedKey(rawKey), It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenRedisThrows_ShouldRethrow()
    {
        _dbMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down"));

        await Should.ThrowAsync<RedisConnectionException>(() => _sut.GetAsync<SamplePayload>("any-key"));
    }

    [Fact]
    public async Task GetAsync_WhenRedisThrows_ShouldLogError()
    {
        var exception = new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down");

        _dbMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(exception);

        await Should.ThrowAsync<RedisConnectionException>(() => _sut.GetAsync<SamplePayload>("any-key"));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Redis GET failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------------
    // RemoveAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task RemoveAsync_ShouldDeleteKeyWithInstanceNamePrefix()
    {
        const string rawKey = "session:5";

        _dbMock
            .Setup(x => x.KeyDeleteAsync(PrefixedKey(rawKey), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        await _sut.RemoveAsync(rawKey);

        _dbMock.Verify(
            x => x.KeyDeleteAsync(PrefixedKey(rawKey), It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WhenRedisThrows_ShouldRethrow()
    {
        _dbMock
            .Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisException("delete failure"));

        await Should.ThrowAsync<RedisException>(() => _sut.RemoveAsync("some-key"));
    }

    [Fact]
    public async Task RemoveAsync_WhenRedisThrows_ShouldLogError()
    {
        var exception = new RedisException("delete failure");

        _dbMock
            .Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(exception);

        await Should.ThrowAsync<RedisException>(() => _sut.RemoveAsync("some-key"));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Redis DELETE failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------------
    // ExistsAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
    {
        const string key = "feature-flag:dark-mode";

        _dbMock
            .Setup(x => x.KeyExistsAsync(PrefixedKey(key), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        var result = await _sut.ExistsAsync(key);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyDoesNotExist_ShouldReturnFalse()
    {
        _dbMock
            .Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        var result = await _sut.ExistsAsync("non-existent");

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ShouldUseKeyWithInstanceNamePrefix()
    {
        const string rawKey = "flag:beta";

        _dbMock
            .Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        await _sut.ExistsAsync(rawKey);

        _dbMock.Verify(
            x => x.KeyExistsAsync(PrefixedKey(rawKey), It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenRedisThrows_ShouldRethrow()
    {
        _dbMock
            .Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.SocketFailure, "conn lost"));

        await Should.ThrowAsync<RedisConnectionException>(() => _sut.ExistsAsync("any-key"));
    }

    [Fact]
    public async Task ExistsAsync_WhenRedisThrows_ShouldLogError()
    {
        var exception = new RedisConnectionException(ConnectionFailureType.SocketFailure, "conn lost");

        _dbMock
            .Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(exception);

        await Should.ThrowAsync<RedisConnectionException>(() => _sut.ExistsAsync("any-key"));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Redis EXISTS failed")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------------
    // Helper
    // ---------------------------------------------------------------------------

    private sealed record SamplePayload(string Name, int Age);
}