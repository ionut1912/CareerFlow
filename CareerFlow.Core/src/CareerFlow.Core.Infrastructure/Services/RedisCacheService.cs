using System.Text.Json;

using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Infrastructure.Configurations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace CareerFlow.Core.Infrastructure.Services;

public partial class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly CacheSettings _settings;

    public RedisCacheService(
        IConnectionMultiplexer connection,
        IOptions<CacheSettings> options,
        ILogger<RedisCacheService> logger)
    {
        _connection = connection;
        _database = _connection.GetDatabase();
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            RedisValue redisValue = await _database.StringGetAsync(PrefixedKey(key));
            return !redisValue.HasValue ? default : JsonSerializer.Deserialize<T>(redisValue.ToString());
        }
        catch (RedisConnectionException ex)
        {
            LogGetError(ex, key);
            throw;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            string json = JsonSerializer.Serialize(value);
            TimeSpan timeToLive = expiry ?? TimeSpan.FromMinutes(_settings.DefaultExpiryMinutes);
            await _database.StringSetAsync(PrefixedKey(key), json, timeToLive);
        }
        catch (RedisConnectionException ex)
        {
            LogSetError(ex, key);
            throw;
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            IServer[] servers = _connection.GetServers();
            if (servers.Length == 0) throw new InvalidOperationException("No Redis servers available.");

            IServer server = servers[0];
            string searchPattern = $"{_settings.InstanceName}{pattern}*";
            IAsyncEnumerable<RedisKey> keys = server.KeysAsync(pattern: searchPattern);

            await foreach (RedisKey key in keys) await _database.KeyDeleteAsync(key);
        }
        catch (RedisConnectionException ex)
        {
            LogRemovePatternError(ex, pattern);
            throw;
        }
    }

    private RedisKey PrefixedKey(string key) => $"{_settings.InstanceName}{key}";

    [LoggerMessage(Level = LogLevel.Error, Message = "Redis connection error while getting key: {Key}")]
    private partial void LogGetError(Exception ex, string key);

    [LoggerMessage(Level = LogLevel.Error, Message = "Redis connection error while setting key: {Key}")]
    private partial void LogSetError(Exception ex, string key);

    [LoggerMessage(Level = LogLevel.Error, Message = "Redis connection error while removing pattern: {Pattern}")]
    private partial void LogRemovePatternError(Exception ex, string pattern);
}
