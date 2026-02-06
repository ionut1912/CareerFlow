using CareerFlow.Core.Domain.Abstractions.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CarrerFlow.Core.Infrastructure.Services;

public class CacheService(IDistributedCache cache) : ICacheService
{
    public async Task<T?> GetCacheValueAsync<T>(string key)
    {
        string? jsonValue = await cache.GetStringAsync(key);
        if(jsonValue == null)
        {
            return default;
        }  

        return JsonSerializer.Deserialize<T>(jsonValue);
    }

    public async Task SetCacheValueAsync<T>(string key, T value)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
            SlidingExpiration=TimeSpan.FromHours(6)
        };

        var jsonValue = JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, jsonValue, options);
    }
}
