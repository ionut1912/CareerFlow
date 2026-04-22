using CareerFlow.Core.Domain.Abstractions.Services;

namespace CareerFlow.Core.Infrastructure.Tests.Unit.Setup;

public sealed class FakeCacheService : ICacheService
{
    private readonly Dictionary<string, object> _store = new();

    public Task<T?> GetAsync<T>(string key)
    {
        _store.TryGetValue(key, out object? value);
        return Task.FromResult(value is T typed ? typed : default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        _store[key] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        var keysToRemove = _store.Keys
            .Where(k => k.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (string key in keysToRemove)
            _store.Remove(key);

        return Task.CompletedTask;
    }

    public bool TryGetValue<T>(string key, out T? value)
    {
        if (_store.TryGetValue(key, out object? obj) && obj is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }
}
