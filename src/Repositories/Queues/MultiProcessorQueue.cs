using RoeiBajayo.Infrastructure.Threads;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Repositories.Queues;

/// <summary>
/// Thread-safe FIFO queue that allows enqueuing items of type T and processing them asynchronously 
/// using a pool of tasks. 
/// The queue has a maximum number of processes that it can run concurrently, specified when the queue is constructed.
/// </summary>
/// <typeparam name="T">Message to proccess</typeparam>
public sealed class MultiProcessorQueue<T> : IDisposable
{
    public bool IsRunning { get; private set; }
    public bool IsDisposed { get; private set; }

    private readonly LockingQueue<T> _queue;
    private readonly ITaskPool _pool;
    private readonly Func<T, Task> _action;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public MultiProcessorQueue(Func<T, Task> processAction,
        int maxProcessesCount)
    {
        if (maxProcessesCount <= 0)
        {
            throw new ArgumentException("Max processes count must be bigger then 0", nameof(maxProcessesCount));
        }

        _action = processAction ?? throw new ArgumentNullException(nameof(processAction));
        _queue = new LockingQueue<T>();
        _pool = Tasks.CreatePool(maxProcessesCount);
    }

    public void Enqueue(T item)
    {
        if (IsDisposed)
            throw new NullReferenceException("This queue is disposed");

        _queue.Enqueue(item);
    }
    public void Enqueue(IEnumerable<T> items)
    {
        if (IsDisposed)
            throw new NullReferenceException("This queue is disposed");

        _queue.Enqueue(items);
    }

    public int Count =>
        _queue.Count;

    public async Task StartAsync()
    {
        if (!IsRunning)
        {
            IsRunning = true;
            _cancellationTokenSource.TryReset();
            while (IsRunning)
            {
                var message = await _queue.DequeueAsync(_cancellationTokenSource.Token);
                if (IsRunning)
                {
                    _pool.Enqueue(async () =>
                    {
                        await _action(message!);
                    });
                }
            }
        }
    }
    public void Stop()
    {
        IsRunning = false;
        _cancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
        if (IsDisposed)
            throw new NullReferenceException("this queue is disposed");

        IsDisposed = true;
        IsRunning = false;
        _cancellationTokenSource.Cancel();
    }
}
