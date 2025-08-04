using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Repositories.Queues;

/// <summary>
/// Thread-safe FIFO queue that lock the TryDequeue method if no elements in the queue
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class LockingQueue<T>
{
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>();
    public int Count => _channel.Reader.Count;

    public void Enqueue(IEnumerable<T> items)
    {
        foreach (var item in items)
            _channel.Writer.TryWrite(item);
    }
    public void Enqueue(T item)
    {
        _channel.Writer.TryWrite(item);
    }


    public IAsyncEnumerable<T> DequeueAllAsync() =>
       DequeueAllAsync(CancellationToken.None);
    public IAsyncEnumerable<T> DequeueAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }

    public ValueTask<T> DequeueAsync() =>
        DequeueAsync(CancellationToken.None);
    public ValueTask<T> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}