namespace CareerFlow.Core.Domain.Abstractions.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveByPatternAsync(string pattern);
}
