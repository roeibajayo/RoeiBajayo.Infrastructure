using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Utils.DependencyInjection.ScopedCache;

public interface IScopedCache
{
    /// <summary>
    /// Gets the keys of all items in the cache.
    /// </summary>
    IEnumerable<string> Keys { get; }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the item to retrieve.</param>
    /// <returns>The value associated with the specified key, or null if the key does not exist.</returns>
    T? Get<T>(string key);

    /// <summary>
    /// Gets the value associated with the specified key, or sets it to the default value if it does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the item to retrieve.</param>
    /// <param name="defaultValue">The default value to set if the key does not exist.</param>
    /// <returns></returns>
    T GetOrSet<T>(string key, T defaultValue);

    /// <summary>
    /// Gets the value associated with the specified key, or sets it to the result of the provided function if it does not exist.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the item to retrieve.</param>
    /// <param name="defaultValue">The default value to set if the key does not exist.</param>
    /// <returns></returns>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> defaultValue);

    /// <summary>
    /// Sets the value associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key of the item to retrieve.</param>
    /// <param name="value">The value to set.</param>
    void Set<T>(string key, T value);
}
