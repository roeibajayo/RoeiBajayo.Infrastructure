using Infrastructure.Utils.Threads.KeyedLocker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Utils.DependencyInjection.ScopedCache;

internal sealed class ScopedCacheManager(IKeyedLocker keyedLocker) : IScopedCache
{
    private Dictionary<string, object>? cache = null;

    public IEnumerable<string> Keys => cache?.Keys.AsEnumerable() ?? [];

    public T? Get<T>(string key)
    {
        if (cache?.TryGetValue(key, out var value) ?? false)
            return (T)value;

        return default;
    }

    public void Set<T>(string key, T value)
    {
        if (value is null)
            return;

        cache ??= [];
        cache[key] = value;
    }

    public T GetOrSet<T>(string key, T defaultValue)
    {
        if (cache is not null && cache.TryGetValue(key, out object? value))
            return (T)value;

        if (defaultValue is null)
            return defaultValue;

        cache ??= [];
        cache[key] = defaultValue;
        return defaultValue;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueGetter)
    {
        if (cache is not null && cache.TryGetValue(key, out object? value))
            return (T)value;

        var result = await keyedLocker.TryExecuteAsync(key, async () =>
        {
            var value = await valueGetter();

            if (value is not null)
            {
                cache ??= [];
                cache[key] = value;
            }

            return value;
        });


        return result;
    }
}
