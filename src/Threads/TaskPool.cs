using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Threads;

/// <summary>
/// pool that allows you to enqueue tasks and runs them concurrently, up to a maximum number specified when the TaskPool is constructed. 
/// It also has support for a CancellationToken, which allows you to cancel the tasks that are currently running or waiting in the pool.
/// </summary>
public sealed class TaskPool(int maxParallelTasks) : ITaskPool
{
    private bool _disposed;
    private readonly SemaphoreSlim _semaphore = new(maxParallelTasks);

    public Task EnqueueAsync(Func<Task> task, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(TaskPool));

        return Tasks.StartNow(async () =>
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                await task().WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // handle the cancellation exception
            }
            catch (Exception)
            {
                // handle other exceptions
            }
            finally
            {
                _semaphore.Release();
            }
        }, cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _semaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}
