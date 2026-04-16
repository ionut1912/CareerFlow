using System.Net;
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
    private readonly Mock<IServer> _serverMock = new();

    private readonly CacheSettings _settings = new()
    {
        InstanceName = InstanceName,
        DefaultExpiryMinutes = DefaultExpiryMinutes
    };

    private readonly RedisCacheService _sut;

    public RedisCacheServiceTests()
    {
        var endpoint = new DnsEndPoint("localhost", 6379);

        _connectionMock
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object?>()))
            .Returns(_dbMock.Object);

        _connectionMock
            .Setup(x => x.GetEndPoints(It.IsAny<bool>()))
            .Returns(new EndPoint[] { endpoint });

        _connectionMock
            .Setup(x => x.GetServer(It.IsAny<EndPoint>(), It.IsAny<object?>()))
            .Returns(_serverMock.Object);

        _sut = new RedisCacheService(
            _connectionMock.Object,
            Options.Create(_settings),
            _loggerMock.Object);
    }

    private static string Key(string raw)
    {
        return $"{InstanceName}{raw}";
    }

    [Fact]
    public async Task GetAsync_WhenKeyExists_ShouldReturnDeserializedValue()
    {
        const string key = "user:42";
        var expected = new SamplePayload("Alice", 30);
        var json = JsonSerializer.Serialize(expected);

        _dbMock
            .Setup(x => x.StringGetAsync(Key(key), It.IsAny<CommandFlags>()))
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
    public async Task GetAsync_WhenRedisThrows_ShouldReturnDefaultWithoutRethrowing()
    {
        _dbMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down"));

        var result = await _sut.GetAsync<SamplePayload>("any-key");

        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAsync_WhenRedisThrows_ShouldLogError()
    {
        var exception = new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down");

        _dbMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(exception);

        await _sut.GetAsync<SamplePayload>("any-key");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("GET")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
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
            x => x.StringGetAsync(Key(rawKey), It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_WhenRedisThrows_ShouldNotRethrow()
    {
        _dbMock
            .Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(), It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisException("write failure"));

        await Should.NotThrowAsync(() => _sut.SetAsync("k", new SamplePayload("X", 0)));
    }

    [Fact]
    public async Task RemoveAsync_WhenRedisThrows_ShouldNotRethrow()
    {
        _dbMock
            .Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisException("delete failure"));

        await Should.NotThrowAsync(() => _sut.RemoveAsync("some-key"));
    }

    [Fact]
    public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
    {
        const string key = "feature-flag:dark-mode";

        _dbMock
            .Setup(x => x.KeyExistsAsync(Key(key), It.IsAny<CommandFlags>()))
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
    public async Task ExistsAsync_WhenRedisThrows_ShouldReturnFalse()
    {
        _dbMock
            .Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.SocketFailure, "conn lost"));

        var result = await _sut.ExistsAsync("any-key");

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WhenRedisThrows_ShouldLogError()
    {
        _dbMock
            .Setup(x => x.KeyExistsAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.SocketFailure, "conn lost"));

        await _sut.ExistsAsync("any-key");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("EXISTS")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private sealed record SamplePayload(string Name, int Age);
}