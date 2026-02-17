using CareerFlow.Core.Infrastructure.Services;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Shouldly;
using System.Text;
using System.Text.Json;
using Xunit;

namespace CareerFlow.Core.Infrastructure.Tests.Unit;

public class CacheServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly CacheService _sut;

    public CacheServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _sut = new CacheService(_cacheMock.Object);
    }

    [Fact]
    public async Task GetCacheValueAsync_KeyNotFound_ReturnsNull()
    {
        // Arrange
        _cacheMock
            .Setup(x => x.GetAsync("missing-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _sut.GetCacheValueAsync<string>("missing-key");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetCacheValueAsync_KeyNotFoundValueType_ReturnsDefaultInt()
    {
        // Arrange
        _cacheMock
            .Setup(x => x.GetAsync("missing-int", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _sut.GetCacheValueAsync<int>("missing-int");

        // Assert
        result.ShouldBe(0);
    }

    [Fact]
    public async Task GetCacheValueAsync_KeyExists_ReturnsDeserializedStringValue()
    {
        // Arrange
        const string expectedValue = "hello world";
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(expectedValue));

        _cacheMock
            .Setup(x => x.GetAsync("my-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _sut.GetCacheValueAsync<string>("my-key");

        // Assert
        result.ShouldBe(expectedValue);
    }

    [Fact]
    public async Task GetCacheValueAsync_ComplexObject_ReturnsCorrectlyDeserialized()
    {
        // Arrange
        var obj = new TestCachePayload { Id = 7, Name = "Alice" };
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));

        _cacheMock
            .Setup(x => x.GetAsync("obj-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _sut.GetCacheValueAsync<TestCachePayload>("obj-key");

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(7);
        result.Name.ShouldBe("Alice");
    }

    [Fact]
    public async Task SetCacheValueAsync_ValidValue_StoresSerializedBytesUnderCorrectKey()
    {
        // Arrange
        const string key = "store-key";
        const string value = "stored-value";
        var expectedJson = JsonSerializer.Serialize(value);

        // Act
        await _sut.SetCacheValueAsync(key, value);

        // Assert
        _cacheMock.Verify(x => x.SetAsync(
            key,
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == expectedJson),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetCacheValueAsync_ValidValue_SetsCorrectExpirationOptions()
    {
        // Arrange
        DistributedCacheEntryOptions? capturedOptions = null;

        // Moq uses .Callback to capture arguments. The callback signature must match the method signature.
        _cacheMock
            .Setup(x => x.SetAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (k, v, o, t) => capturedOptions = o);

        // Act
        await _sut.SetCacheValueAsync("key", "value");

        // Assert
        capturedOptions.ShouldNotBeNull();
        capturedOptions.AbsoluteExpirationRelativeToNow.ShouldBe(TimeSpan.FromHours(24));
        capturedOptions.SlidingExpiration.ShouldBe(TimeSpan.FromHours(6));
    }

    [Fact]
    public async Task SetCacheValueAsync_ComplexObject_StoresCorrectlySerializedJson()
    {
        // Arrange
        const string key = "complex-key";
        var payload = new TestCachePayload { Id = 3, Name = "Bob" };
        var expectedJson = JsonSerializer.Serialize(payload);

        // Act
        await _sut.SetCacheValueAsync(key, payload);

        // Assert
        _cacheMock.Verify(x => x.SetAsync(
            key,
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == expectedJson),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetCacheValueAsync_NullableValue_StoresNullJson()
    {
        // Arrange
        string? nullValue = null;
        var expectedJson = JsonSerializer.Serialize(nullValue);

        // Act
        await _sut.SetCacheValueAsync<string?>("null-key", nullValue);

        // Assert
        _cacheMock.Verify(x => x.SetAsync(
            "null-key",
            It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == expectedJson),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed record TestCachePayload
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}