using System.Text.Json;
using CareerFlow.Core.Domain.Abstractions.Services;
using CareerFlow.Core.Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace CareerFlow.Core.Infrastructure.Services;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ILogger<RedisCacheService> _logger;
    private readonly CacheSettings _options;

    public RedisCacheService(
        IConnectionMultiplexer connection,
        IOptions<CacheSettings> options,
        ILogger<RedisCacheService> logger)
    {
        _db = connection.GetDatabase();
        _options = options.Value;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var value = await _db.StringGetAsync(BuildKey(key));
            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>((string)value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis GET failed for key {Key}", key);
            throw;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value, _jsonOptions);
            var ttl = expiry ?? TimeSpan.FromMinutes(_options.DefaultExpiryMinutes);
            await _db.StringSetAsync(BuildKey(key), serialized, ttl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis SET failed for key {Key}", key);
            throw;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _db.KeyDeleteAsync(BuildKey(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis DELETE failed for key {Key}", key);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        try
        {
            return await _db.KeyExistsAsync(BuildKey(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis EXISTS failed for key {Key}", key);
            throw;
        }
    }

    private string BuildKey(string key) => $"{_options.InstanceName}{key}";
}