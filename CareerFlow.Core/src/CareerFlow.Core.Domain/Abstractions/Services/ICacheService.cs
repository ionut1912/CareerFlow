namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ICacheService
{
    Task SetCacheValueAsync<T>(string key, T value);
    Task<T?> GetCacheValueAsync<T>(string key);
}
