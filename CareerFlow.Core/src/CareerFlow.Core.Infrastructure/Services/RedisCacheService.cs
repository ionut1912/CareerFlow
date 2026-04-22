using System.Text.Json;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CareerFlow.Core.Infrastructure.Services;

public sealed partial class RedisCacheService(
    IConnectionMultiplexer connection,
    IOptions<CacheSettings> options,
    ILogger<RedisCacheService> logger)
    : ICacheService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IDatabase _db = connection.GetDatabase();
    private readonly CacheSettings _options = options.Value;

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            RedisValue value = await _db.StringGetAsync(BuildKey(key));
            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>((string)value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            LogGetFailed(ex, key);
            throw;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            string serialized = JsonSerializer.Serialize(value, _jsonOptions);
            TimeSpan ttl = expiry ?? TimeSpan.FromMinutes(_options.DefaultExpiryMinutes);
            await _db.StringSetAsync(BuildKey(key), serialized, ttl);
        }
        catch (Exception ex)
        {
            LogSetFailed(ex, key);
            throw;
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            IServer server = _db.Multiplexer.GetServers().FirstOrDefault()
                             ?? throw new InvalidOperationException("No Redis server available");

            IAsyncEnumerable<RedisKey> keys = server.KeysAsync(pattern: BuildKey(pattern) + "*");

            await foreach (RedisKey key in keys)
                await _db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            LogDeleteByPatternFailed(ex, pattern);
            throw;
        }
    }

    private string BuildKey(string key) => $"{_options.InstanceName}{key}";

    [LoggerMessage(Level = LogLevel.Error, Message = "Redis GET failed for key {Key}")]
    private partial void LogGetFailed(Exception ex, string key);

    [LoggerMessage(Level = LogLevel.Error, Message = "Redis SET failed for key {Key}")]
    private partial void LogSetFailed(Exception ex, string key);

    [LoggerMessage(Level = LogLevel.Error, Message = "Redis DELETE by pattern failed for pattern {Pattern}")]
    private partial void LogDeleteByPatternFailed(Exception ex, string pattern);
}
