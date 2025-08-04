using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Threads.KeyedLocker;

public interface IKeyedLocker
{
    /// <summary>
    /// Keyed locker for executing functions with a specific key, allowing only one execution at a time for that key.
    /// </summary>
    /// <param name="key">The key to identify the lock.</param>
    /// <param name="func">The function to execute.</param>
    /// <param name="force">Whether to force execution even if the key is already locked.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns></returns>
    Task TryExecuteAsync(string key, Func<Task> func, bool force = false, CancellationToken cancellationToken = default);


    /// <summary>
    /// Keyed locker for executing functions with a specific key, allowing only one execution at a time for that key.
    /// </summary>
    /// <param name="key">The key to identify the lock.</param>
    /// <param name="func">The function to execute.</param>
    /// <param name="force">Whether to force execution even if the key is already locked.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of the function execution.</returns>
    Task<T> TryExecuteAsync<T>(string key, Func<Task<T>> func, bool force = false, CancellationToken cancellationToken = default);
}