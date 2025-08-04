using Infrastructure.Utils.Dates;
using Infrastructure.Utils.Repositories.Queues.Throttling.Models;
using Infrastructure.Utils.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Utils.Repositories.Queues.Throttling;

public abstract class ThrottlingQueueBase<T>
{
    protected SemaphoreSlim? WaitingLocker;
    protected bool Running;
    private DateTimeOffset? WaitingUntil;

    public ThrottlingQueueBase(ThrottlingTimeSpan[] dateParts)
    {
        if (dateParts is null or { Length: 0 })
            throw new ArgumentOutOfRangeException(nameof(dateParts));

        foreach (var unit in dateParts)
            if (unit == null || unit.TimeSpan.TotalSeconds <= 0 || unit.MaxExecutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(dateParts));

        DateParts = dateParts;
    }

    protected ThrottlingTimeSpan[] DateParts { get; }

    public virtual async Task<IEnumerable<T>> TryDequeueAsync()
    {
        var completed = -1;
        var count = -1;
        foreach (var unit in DateParts)
        {
            var unitCompleted = await CountCurrentDequeuedAsync(unit);
            completed += unitCompleted;
            var unitCount = unit.MaxExecutes - unitCompleted;

            if (count == -1 || unitCount < count)
            {
                count = unitCount;
            }
        }

        var items = count <= 0 ?
            [] :
            await DequeueItemsAsync(count);
        return items;
    }
    public async Task<int> CountCurrentDequeuedAsync(EnhancedTimeSpan unit)
    {
        var from = unit.GetLastStart();
        return await CountDequeuedAsync(from);
    }

    protected abstract Task<int> CountDequeuedAsync(DateTimeOffset from);
    protected abstract Task<IEnumerable<T>> DequeueItemsAsync(int count);
    protected abstract Task<bool> AnyInQueueAsync();
    protected abstract Task<DateTimeOffset?> GetLastExecuteAsync(DateTimeOffset minExecuteDate);


    public async Task<bool> TryStartAsync(Func<IEnumerable<T>, Task> onItemDequeueAsync) =>
        await TryStartAsync(onItemDequeueAsync, false);
    public async Task<bool> TryWaitForEndAsync(Func<IEnumerable<T>, Task> onItemDequeueAsync) =>
        await TryStartAsync(onItemDequeueAsync, true);

    private async Task<bool> TryStartAsync(Func<IEnumerable<T>, Task> onItemDequeueAsync, bool breakOnHold)
    {
        if (Running)
            return false;

        WaitingLocker?.Dispose();
        WaitingLocker = new SemaphoreSlim(1);
        WaitingLocker.Wait();

        Running = true;
        await RunAsync(onItemDequeueAsync, breakOnHold);
        Running = false;
        return true;
    }
    private async Task RunAsync(Func<IEnumerable<T>, Task> onItemDequeueAsync, bool breakOnHold)
    {
        var alreadyFoundItems = false;
        async Task waitForNextLoopAsync()
        {
            if (WaitingLocker!.CurrentCount == 1)
                WaitingLocker.Wait();
            await WaitingLocker.WaitAsync();
        }

        while (Running)
        {
            WaitingUntil = null;

            if (!await AnyInQueueAsync())
            {
                if (breakOnHold && alreadyFoundItems)
                {
                    break;
                }
                else
                {
                    //waiting for first items
                    await waitForNextLoopAsync();
                }
            }

            //Console.WriteLine($"[{DateTime.Now}] Try dequeue..");
            var dequeuedItems = await TryDequeueAsync();
            var dequeuedItemsCount = dequeuedItems.CountImproved();

            //Console.WriteLine($"[{DateTime.Now}] Total dequeue items: {dequeuedItemsCount}");

            if (dequeuedItemsCount == 0)
            {
                if (!await AnyInQueueAsync())
                    continue; //released not in time, maybe async task released the running

                //cant dequeue, max items
                var next = await GetNextExecutionAsync();
                if (next != null)
                {
                    var now = DateTimeOffset.Now;
                    var until = next.Value - now;
                    if (WaitingUntil == null || next > WaitingUntil)
                    {
                        WaitingUntil = next;
                    }
                    else
                    {
                        await waitForNextLoopAsync();
                        continue;
                    }

                    _ = Tasks.StartNow(async () =>
                    {
                        await Tasks.DelayUntil(next.Value + TimeSpan.FromMilliseconds(20));
                        WaitingLocker!.Release();
                    });

                    await waitForNextLoopAsync();
                }
            }
            else
            {
                alreadyFoundItems = true;

                if (Running)
                    await onItemDequeueAsync(dequeuedItems);
            }
        }
    }
    protected void ReleaseIfRunning()
    {
        if (Running && WaitingUntil == null)
            WaitingLocker!.Release();
    }
    public void Stop()
    {
        Running = false;
        WaitingLocker!.Release();
        WaitingLocker.Dispose();
    }

    private async Task<DateTimeOffset?> GetNextExecutionAsync()
    {
        var now = DateTimeOffset.Now;
        var minExecuteDate = DateParts.Min(unit => unit.GetLastStart(now));
        var lastExecuted = await GetLastExecuteAsync(minExecuteDate);

        var shortestDate = DateParts
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
}
