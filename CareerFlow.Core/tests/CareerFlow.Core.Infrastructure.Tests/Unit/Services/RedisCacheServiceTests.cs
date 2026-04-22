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
        ConnectionString = "test-con-string",
        InstanceName = InstanceName,
        DefaultExpiryMinutes = DefaultExpiryMinutes,
        AbortOnConnectFail = false
    };

    private readonly RedisCacheService _sut;

    public RedisCacheServiceTests()
    {
        _connectionMock
            .Setup(x => x.GetDatabase(It.IsAny<int>(), It.IsAny<object?>()))
            .Returns(_dbMock.Object);

        _connectionMock
            .Setup(x => x.GetServers())
            .Returns([_serverMock.Object]);

        _dbMock
            .Setup(x => x.Multiplexer)
            .Returns(_connectionMock.Object);

        _sut = new RedisCacheService(
            _connectionMock.Object,
            Options.Create(_settings),
            _loggerMock.Object);
    }

    private static RedisKey PrefixedKey(string raw) => $"{InstanceName}{raw}";

    // ---------------------------------------------------------------------------
    // GetAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_WhenKeyExists_ShouldReturnDeserializedValue()
    {
        // Arrange
        const string key = "user:42";
        var expected = new SamplePayload("Alice", 30);
        string json = JsonSerializer.Serialize(expected);

        _dbMock
            .Setup(x => x.StringGetAsync(PrefixedKey(key), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)json);

        // Act
        SamplePayload? result = await _sut.GetAsync<SamplePayload>(key);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(expected.Name);
        result.Age.ShouldBe(expected.Age);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnDefault()
    {
        // Arrange
        _dbMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        SamplePayload? result = await _sut.GetAsync<SamplePayload>("missing-key");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetAsync_ShouldUseKeyWithInstanceNamePrefix()
    {
        // Arrange
        const string rawKey = "session:99";

        _dbMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act
        await _sut.GetAsync<SamplePayload>(rawKey);

        // Assert
        _dbMock.Verify(
            x => x.StringGetAsync(PrefixedKey(rawKey), It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenRedisThrows_ShouldRethrow()
    {
        // Arrange
        _dbMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down"));

        // Act & Assert
        await Should.ThrowAsync<RedisConnectionException>(() => _sut.GetAsync<SamplePayload>("any-key"));
    }

    [Fact]
    public async Task GetAsync_WhenRedisThrows_ShouldLogError()
    {
        // Arrange
        var exception = new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down");

        _dbMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(exception);

        // Act
        await Should.ThrowAsync<RedisConnectionException>(() => _sut.GetAsync<SamplePayload>("any-key"));

        // Assert
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
    // RemoveByPatternAsync
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task RemoveByPatternAsync_ShouldDeleteAllMatchingKeys()
    {
        // Arrange
        const string pattern = "course:analyze:";
        var matchingKeys = new RedisKey[] { "myapp:course:analyze:file1", "myapp:course:analyze:file2" };

        _serverMock
            .Setup(x => x.KeysAsync(
                It.IsAny<int>(),
                It.Is<RedisValue>(v => v.ToString().Contains(pattern)),
                It.IsAny<int>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                It.IsAny<CommandFlags>()))
            .Returns(matchingKeys.ToAsyncEnumerable());

        _dbMock
            .Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _sut.RemoveByPatternAsync(pattern);

        // Assert
        _dbMock.Verify(
            x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()),
            Times.Exactly(matchingKeys.Length));
    }

    [Fact]
    public async Task RemoveByPatternAsync_WhenNoKeysMatch_ShouldNotDelete()
    {
        // Arrange
        _serverMock
            .Setup(x => x.KeysAsync(
                It.IsAny<int>(),
                It.IsAny<RedisValue>(),
                It.IsAny<int>(),
                It.IsAny<long>(),
                It.IsAny<int>(),
                It.IsAny<CommandFlags>()))
            .Returns(AsyncEnumerable.Empty<RedisKey>());

        // Act
        await _sut.RemoveByPatternAsync("nonexistent:");

        // Assert
        _dbMock.Verify(
            x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()),
            Times.Never);
    }

    [Fact]
    public async Task RemoveByPatternAsync_WhenNoServerAvailable_ShouldThrow()
    {
        // Arrange
        _connectionMock
            .Setup(x => x.GetServers())
            .Returns([]);

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(() => _sut.RemoveByPatternAsync("course:"));
    }

    // ---------------------------------------------------------------------------
    // Helper
    // ---------------------------------------------------------------------------

    private sealed record SamplePayload(string Name, int Age);
}
