using RoeiBajayo.Infrastructure.Repositories.Queues.Throttling.Models;
using RoeiBajayo.Infrastructure.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Repositories.Queues.Throttling;

/// <summary>
/// This ThrottlingCounter class allows you to specify a maximum number of requests per time period, 
/// and the TryEnqueue method will return true if the current number of requests is within the 
/// limit for the time period. 
/// If the limit has been reached, TryEnqueue will return false so you can choose to either 
/// discard the request or try again later.
/// </summary>
public sealed class ThrottlingCounter
{
    private readonly ThrottlingTimeSpan[] ThrottlingWindows;
    private readonly Queue<DateTimeOffset> Executes;
#if NET9_0_OR_GREATER
    private readonly Lock LOCKER = new();
#else
    private readonly object LOCKER = new();
#endif

    public ThrottlingCounter(ThrottlingTimeSpan throttlingWindow) :
        this([throttlingWindow])
    { }
    public ThrottlingCounter(ThrottlingTimeSpan[] throttlingWindows)
    {
        if (throttlingWindows == null || throttlingWindows.Length == 0)
            throw new ArgumentOutOfRangeException(nameof(throttlingWindows));

        foreach (var unit in throttlingWindows)
            if (unit == null || unit.TimeSpan.TotalSeconds <= 0 || unit.MaxExecutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(throttlingWindows));

        ThrottlingWindows = throttlingWindows;
        Executes = new Queue<DateTimeOffset>();
    }

    public bool TryEnqueue()
    {
        lock (LOCKER)
        {
            ClearOldExecutes();

            var now = DateTimeOffset.Now;
            foreach (var window in ThrottlingWindows)
            {
                // count how many executes are in the time window
                var from = window.GetLastStart(now);
                var count = Executes.Count(x => x > from);
                var left = window.MaxExecutes - count;

                if (left <= 0)
                    return false;
            }

            Executes.Enqueue(now);
            return true;
        }
    }
    private void ClearOldExecutes()
    {
        if (Executes.Count < 15)
            return;

        var now = DateTimeOffset.Now;
        var removeBefore = ThrottlingWindows.Min(unit => unit.GetLastStart(now));
        while (Executes.Count != 0 && removeBefore > Executes.Peek())
        {
            Executes.Dequeue();
        }
    }

    public async Task WaitForEnqueueAsync(CancellationToken cancellationToken)
    {
        while (!TryEnqueue())
        {
            //cant enqueue, max items
            var next = GetNextExecution();
            if (next != null)
            {
                await Tasks.DelayUntil(next.Value + TimeSpan.FromMilliseconds(20), cancellationToken);
            }
        }
    }
    private DateTimeOffset? GetNextExecution()
    {
        var now = DateTimeOffset.Now;
        var lastExecuted = Executes.Count == 0 ? (DateTimeOffset?)null : Executes.Peek();

        var shortestDate = ThrottlingWindows
            .Where(x => (lastExecuted != null || x.Fixed) && // ignore not fixed if not last execute
                (x.Fixed ? x.GetLastStart(now) : lastExecuted) + x.TimeSpan > now)
            .MinBy(x => x.Fixed ?
                (now - x.GetLastStart(now) + x.TimeSpan).TotalMilliseconds :
                x.TimeSpan.TotalMilliseconds);

        DateTimeOffset? next = null;
        if (shortestDate!.Fixed)
        {
            next = shortestDate.GetLastStart(now) + shortestDate.TimeSpan;
        }
        else if (lastExecuted != null)
        {
            next = lastExecuted + shortestDate.TimeSpan;
        }
        return next;
    }

    public DateTimeOffset? LastEnqueue
    {
        get
        {
            ClearOldExecutes();

            if (Executes.Count == 0)
                return null;

            return Executes.Peek();
        }
    }

    public int Count
    {
        get
        {
            ClearOldExecutes();
            return Executes.Count;
        }
    }

    public void Clear()
    {
        Executes.Clear();
    }
}
