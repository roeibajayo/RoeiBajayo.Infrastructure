using Infrastructure.Utils.DependencyInjection.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Utils.Threads.KeyedLocker;

public class KeyedLocker : ISingletonService<IKeyedLocker>, IKeyedLocker
{
    private readonly ConcurrentDictionary<string, Task> executings = [];
#if NET9_0_OR_GREATER
    private readonly Lock asyncDictionaryLocker = new();
#else
    private readonly object asyncDictionaryLocker = new();
#endif

    public async Task<T> TryExecuteAsync<T>(
        string key,
        Func<Task<T>> func,
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        if (force)
            return await func();

        var completed = (Task<T>)await WaitForCompletedAsync(key, func, cancellationToken);
        var result = completed.Result;
        return result;
    }
    public async Task TryExecuteAsync(string key,
        Func<Task> func,
        bool force = false, 
        CancellationToken cancellationToken = default)
    {
        if (force)
        {
            await func();
            return;
        }

        await WaitForCompletedAsync(key, func, cancellationToken);
    }

    private async Task<Task> WaitForCompletedAsync(
        string key,
        Func<Task> func,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        var withCancellation = cancellationToken != CancellationToken.None;
        Task task;
        lock (asyncDictionaryLocker)
        {
            task = executings.GetOrAdd(key, _ => func());
        }

        var completed = task;

        try
        {
            if (withCancellation && task.Status == TaskStatus.Running)
            {
                using var cancellationTaskCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                completed = await Task.WhenAny([
                    task,
                Task.Run(async () => await Task.Delay(Timeout.Infinite, cancellationToken), cancellationToken)
                ]);
                cancellationTaskCts.Cancel();
                await completed;
            }
            else
            {
                await completed;
            }
        }
        finally
        {
            executings.TryRemove(key, out _);
        }

        if (completed.Exception is not null)
            throw completed.Exception;

        if (completed.IsCanceled)
            throw new TaskCanceledException();

        return completed;
    }
}
