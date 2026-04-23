using System.Text.Json;
using CareerFlow.Core.Infrastructure.Configurations;
using CareerFlow.Core.Infrastructure.Services;
using CareerFlow.Core.Infrastructure.Tests.Unit.Setup;

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
    private readonly FakeLogger<RedisCacheService> _fakeLogger = new();
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
            _fakeLogger);
    }

    private static RedisKey PrefixedKey(string raw) => $"{InstanceName}{raw}";

    /// <summary>
    /// Sets up StringSetAsync with the StackExchange.Redis 2.12.x signature
    /// (Expiration instead of TimeSpan?, ValueCondition instead of When) to return true.
    /// </summary>
    private void SetupStringSetAsyncReturnsTrue()
    {
        _dbMock
            .Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<Expiration>(),
                It.IsAny<ValueCondition>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);
    }

    /// <summary>
    /// Sets up StringSetAsync with the StackExchange.Redis 2.12.x signature to throw.
    /// </summary>
    private void SetupStringSetAsyncThrows(Exception ex)
    {
        _dbMock
            .Setup(x => x.StringSetAsync(
                It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<Expiration>(),
                It.IsAny<ValueCondition>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(ex);
    }

    /// <summary>
    /// Returns the single StringSetAsync invocation from the mock's log.
    /// </summary>
    private IInvocation GetSingleStringSetAsyncInvocation()
    {
        var calls = _dbMock.Invocations
            .Where(i => i.Method.Name == nameof(IDatabase.StringSetAsync))
            .ToList();

        calls.Count.ShouldBe(1, "Expected exactly one StringSetAsync call");
        return calls[0];
    }

    // ── GetAsync ─────────────────────────────────────────────────────────────

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
        const string key = "error-key";

        _dbMock
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down"));

        // Act
        await Should.ThrowAsync<RedisConnectionException>(() => _sut.GetAsync<SamplePayload>(key));

        // Assert
        FakeLogRecord record = _fakeLogger.Records.ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Error);
        record.Message.ShouldContain(key);
    }

    // ── SetAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SetAsync_ShouldStoreSerializedValueInRedis()
    {
        // Arrange
        const string key = "user:1";
        var payload = new SamplePayload("Bob", 25);

        SetupStringSetAsyncReturnsTrue();

        // Act
        await _sut.SetAsync(key, payload);

        // Assert
        IInvocation call = GetSingleStringSetAsyncInvocation();

        call.Arguments[0].ToString().ShouldBe(PrefixedKey(key).ToString());

        string actualJson = call.Arguments[1].ToString()!;
        SamplePayload? deserialized = JsonSerializer.Deserialize<SamplePayload>(actualJson);
        deserialized.ShouldNotBeNull();
        deserialized.Name.ShouldBe("Bob");
        deserialized.Age.ShouldBe(25);
    }

    [Fact]
    public async Task SetAsync_ShouldUseKeyWithInstanceNamePrefix()
    {
        // Arrange
        const string rawKey = "token:abc";

        SetupStringSetAsyncReturnsTrue();

        // Act
        await _sut.SetAsync(rawKey, "value");

        // Assert
        IInvocation call = GetSingleStringSetAsyncInvocation();
        call.Arguments[0].ToString().ShouldBe(PrefixedKey(rawKey).ToString());
    }

    [Fact]
    public async Task SetAsync_WhenExpiryProvided_ShouldUseProvidedExpiry()
    {
        // Arrange
        var customExpiry = TimeSpan.FromMinutes(5);

        SetupStringSetAsyncReturnsTrue();

        // Act
        await _sut.SetAsync("key", "value", customExpiry);

        // Assert
        IInvocation call = GetSingleStringSetAsyncInvocation();

        // StackExchange.Redis 2.12.x uses Expiration (implicit from TimeSpan)
        var actualExpiration = (Expiration)call.Arguments[2];
        Expiration expectedExpiration = customExpiry;
        actualExpiration.ShouldBe(expectedExpiration);
    }

    [Fact]
    public async Task SetAsync_WhenExpiryNotProvided_ShouldUseDefaultFromSettings()
    {
        // Arrange
        SetupStringSetAsyncReturnsTrue();

        // Act
        await _sut.SetAsync("key", "value");

        // Assert
        IInvocation call = GetSingleStringSetAsyncInvocation();

        // StackExchange.Redis 2.12.x uses Expiration (implicit from TimeSpan)
        var actualExpiration = (Expiration)call.Arguments[2];
        Expiration expectedExpiration = TimeSpan.FromMinutes(DefaultExpiryMinutes);
        actualExpiration.ShouldBe(expectedExpiration);
    }

    [Fact]
    public async Task SetAsync_WhenRedisThrows_ShouldRethrow()
    {
        // Arrange
        SetupStringSetAsyncThrows(
            new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down"));

        // Act & Assert
        await Should.ThrowAsync<RedisConnectionException>(() => _sut.SetAsync("key", "value"));
    }

    [Fact]
    public async Task SetAsync_WhenRedisThrows_ShouldLogError()
    {
        // Arrange
        const string key = "my-key";

        SetupStringSetAsyncThrows(
            new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down"));

        // Act
        await Should.ThrowAsync<RedisConnectionException>(() => _sut.SetAsync(key, "value"));

        // Assert
        FakeLogRecord record = _fakeLogger.Records.ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Error);
        record.Message.ShouldContain(key);
    }

    // ── RemoveByPatternAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task RemoveByPatternAsync_ShouldPassPrefixedPatternWithWildcardToServer()
    {
        // Arrange
        const string pattern = "course:analyze:";
        const string expectedFullPattern = $"{InstanceName}{pattern}*";

        _serverMock
            .Setup(x => x.KeysAsync(
                It.IsAny<int>(), It.IsAny<RedisValue>(),
                It.IsAny<int>(), It.IsAny<long>(),
                It.IsAny<int>(), It.IsAny<CommandFlags>()))
            .Returns(AsyncEnumerable.Empty<RedisKey>());

        // Act
        await _sut.RemoveByPatternAsync(pattern);

        // Assert
        _serverMock.Verify(
            x => x.KeysAsync(
                It.IsAny<int>(),
                It.Is<RedisValue>(v => v.ToString() == expectedFullPattern),
                It.IsAny<int>(), It.IsAny<long>(),
                It.IsAny<int>(), It.IsAny<CommandFlags>()),
            Times.Once);
    }

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
                It.IsAny<int>(), It.IsAny<long>(),
                It.IsAny<int>(), It.IsAny<CommandFlags>()))
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
                It.IsAny<int>(), It.IsAny<RedisValue>(),
                It.IsAny<int>(), It.IsAny<long>(),
                It.IsAny<int>(), It.IsAny<CommandFlags>()))
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

    [Fact]
    public async Task RemoveByPatternAsync_WhenRedisThrows_ShouldRethrow()
    {
        // Arrange
        _serverMock
            .Setup(x => x.KeysAsync(
                It.IsAny<int>(), It.IsAny<RedisValue>(),
                It.IsAny<int>(), It.IsAny<long>(),
                It.IsAny<int>(), It.IsAny<CommandFlags>()))
            .Throws(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down"));

        // Act & Assert
        await Should.ThrowAsync<RedisConnectionException>(() => _sut.RemoveByPatternAsync("any:"));
    }

    [Fact]
    public async Task RemoveByPatternAsync_WhenRedisThrows_ShouldLogError()
    {
        // Arrange
        const string pattern = "fail:";

        _serverMock
            .Setup(x => x.KeysAsync(
                It.IsAny<int>(), It.IsAny<RedisValue>(),
                It.IsAny<int>(), It.IsAny<long>(),
                It.IsAny<int>(), It.IsAny<CommandFlags>()))
            .Throws(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down"));

        // Act
        await Should.ThrowAsync<RedisConnectionException>(() => _sut.RemoveByPatternAsync(pattern));

        // Assert
        FakeLogRecord record = _fakeLogger.Records.ShouldHaveSingleItem();
        record.Level.ShouldBe(LogLevel.Error);
        record.Message.ShouldContain(pattern);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private sealed record SamplePayload(string Name, int Age);
}
