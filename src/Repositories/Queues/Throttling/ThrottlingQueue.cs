using RoeiBajayo.Infrastructure.Repositories.Queues.Throttling.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Repositories.Queues.Throttling;

/// <summary>
/// ThrottlingQueue that represents a queue that enforces a throttling policy on the items it holds. 
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ThrottlingQueue<T>(ThrottlingTimeSpan[] dateParts) : ThrottlingQueueBase<T>(dateParts)
{
    private readonly ConcurrentQueue<T> Queue = new();
    private readonly Queue<DateTimeOffset> Executes = new();

    /// <summary>
    /// This method adds a single item to the queue.
    /// </summary>
    /// <param name="item"></param>
    public void Enqueue(T item)
    {
        Queue.Enqueue(item);
        ReleaseIfRunning();
        //Console.WriteLine($"[{DateTime.Now}] item added");
    }

    /// <summary>
    /// This method adds multiple items to the queue at once.
    /// </summary>
    /// <param name="items"></param>
    public void Enqueue(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            Queue.Enqueue(item);
        }
        ReleaseIfRunning();
        //Console.WriteLine($"[{DateTime.Now}] items added");
    }

    protected override Task<int> CountDequeuedAsync(DateTimeOffset from)
    {
        return Task.FromResult(DateParts.Length == 1 ?
            Executes.Count : //because all archived executes already removed
            Executes.Count(x => x > from));
    }
    protected override Task<IEnumerable<T>> DequeueItemsAsync(int count)
    {
        var items = new List<T>(count);
        for (var i = 0; i < count; i++)
        {
            if (!Queue.TryDequeue(out var item))
                break;
            items.Add(item);
        }
        return Task.FromResult(items.AsEnumerable());
    }

    /// <summary>
    /// This method attempts to dequeue items from the queue according to the throttling policy.
    /// </summary>
    /// <returns></returns>
    public override async Task<IEnumerable<T>> TryDequeueAsync()
    {
        ClearOldCompletedFromQueue();
        var items = await base.TryDequeueAsync();
        var now = DateTimeOffset.Now;
        for (var i = 0; i < items.CountImproved(); i++)
            Executes.Enqueue(now);
        return items;
    }
    protected override Task<bool> AnyInQueueAsync()
    {
        return Task.FromResult(!Queue.IsEmpty);
    }
    protected override Task<DateTimeOffset?> GetLastExecuteAsync(DateTimeOffset minExecuteDate)
    {
        return Task.FromResult(Executes.Count == 0 ? (DateTimeOffset?)null : Executes.Peek());
    }


#if NET9_0_OR_GREATER
    private readonly Lock LOCKER = new();
#else
    private readonly object LOCKER = new();
#endif
    private void ClearOldCompletedFromQueue()
    {
        lock (LOCKER)
        {
            var now = DateTimeOffset.Now;
            var removeBefore = DateParts.Min(unit => unit.GetLastStart(now));
            while (Executes.Count != 0 && removeBefore > Executes.Peek())
            {
                Executes.Dequeue();
            }
        }
    }

    /// <summary>
    /// Removes all items from the queue and clears the history of items that have been dequeued.
    /// </summary>
    public void Clear()
    {
        Queue.Clear();
        Executes.Clear();
    }
    /// <summary>
    /// Enumeration of the items currently in the queue.
    /// </summary>
    public IEnumerable<T> QueuedItems => Queue;
    /// <summary>
    /// The number of items currently in the queue.
    /// </summary>
    public int CountQueuedItems => Queue.Count;
}
