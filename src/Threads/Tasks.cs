using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Threads;

public static partial class Tasks
{
    private static TimeSpan? GetOffset(DateTimeOffset now, DateTimeOffset date) =>
        date < now ? null : date - now;
    private static TimeSpan? GetOffsetFromNow(DateTimeOffset date) =>
        GetOffset(DateTimeOffset.Now, date);

    public static Task StartNow(Action action) =>
        StartNow(action, CancellationToken.None);
    public static Task StartNow(Action action, CancellationToken cancellationToken) =>
        Task.Run(() => action(), cancellationToken);

    public static Task<T> StartNow<T>(Func<T> task) =>
        StartNow(task, CancellationToken.None);
    public static Task<T> StartNow<T>(Func<T> task, CancellationToken cancellationToken) =>
        Task.Run(task, cancellationToken);

    public static Task StartNow(Func<Task> task) =>
        StartNow(task, CancellationToken.None);
    public static Task StartNow(Func<Task> task, CancellationToken cancellationToken) =>
        StartWithTimeout(task, cancellationToken);
    public static Task<T> StartNow<T>(Func<Task<T>> task) =>
        StartNow(task, CancellationToken.None);
    public static Task<T> StartNow<T>(Func<Task<T>> task, CancellationToken cancellationToken) =>
        StartWithTimeout(task, cancellationToken);

    public static Task StartAt(Action action, DateTime date) =>
        StartAt(action, date, CancellationToken.None);
    public static Task StartAt(Action action, DateTime date, CancellationToken cancellationToken) =>
        StartAt(action, new DateTimeOffset(date), cancellationToken);

    public static Task StartAt(Action action, DateTimeOffset date) =>
        StartAt(action, date, CancellationToken.None);
    public static Task StartAt(Action action, DateTimeOffset date, CancellationToken cancellationToken)
    {
        var offset = GetOffsetFromNow(date);

        if (offset is null)
            return Task.Run(() => action(), cancellationToken);

        return Task.Run(async () => await Task.Delay(offset!.Value, cancellationToken), cancellationToken)
            .ContinueWith((t) => action(), cancellationToken);
    }

    public static Task StartAt(Func<Task> asyncFunc, DateTime date) =>
        StartAt(asyncFunc, date, CancellationToken.None);
    public static Task StartAt(Func<Task> asyncFunc, DateTime date, CancellationToken cancellationToken) =>
        StartAt(asyncFunc, new DateTimeOffset(date), cancellationToken);

    public static Task StartAt(Func<Task> asyncFunc, DateTimeOffset date) =>
        StartAt(asyncFunc, date, CancellationToken.None);
    public static Task StartAt(Func<Task> asyncFunc, DateTimeOffset date, CancellationToken cancellationToken)
    {
        var offset = GetOffsetFromNow(date);

        if (offset is null)
            return Task.Run(() => asyncFunc(), cancellationToken);

        return Task.Run(async () => await Task.Delay(offset.Value, cancellationToken), cancellationToken)
            .ContinueWith(async t => await StartWithTimeout(asyncFunc, cancellationToken));
    }

    public static Task DelayUntil(DateTime date) =>
        DelayUntil(new DateTimeOffset(date), CancellationToken.None);
    public static Task DelayUntil(DateTime date, CancellationToken cancellationToken) =>
        DelayUntil(new DateTimeOffset(date), cancellationToken);
    public static Task DelayUntil(DateTimeOffset date) =>
        DelayUntil(date, CancellationToken.None);
    public static Task DelayUntil(DateTimeOffset date, CancellationToken cancellationToken)
    {
        var offset = GetOffsetFromNow(date);

        if (offset is null)
            return Task.CompletedTask;

        return Task.Delay((int)offset.Value.TotalMilliseconds, cancellationToken);
    }
    public static Task DelayUntil(DateTime currentTime, DateTime date, CancellationToken cancellationToken = default)
    {
        var offset = GetOffset(currentTime, date);

        if (offset is null)
            return Task.CompletedTask;

        return Task.Delay((int)offset.Value.TotalMilliseconds, cancellationToken);
    }

    public static async IAsyncEnumerable<T> WhenEachAsync<T>(IEnumerable<Task<T>> tasks)
    {
        //Tried: IList, Array + Resize, ArraySegment, HashSet, Channel, SemaphoreSlim + ConcurrentBag
        if (tasks is null)
            yield break;

        var left = tasks.ToList();
        while (left.Count > 0)
        {
            var completedTask = await Task.WhenAny(left);
            yield return completedTask.Result;
            left.Remove(completedTask);
        }
    }

    public static ITaskPool CreatePool(int maxParallelTasks) =>
        new TaskPool(maxParallelTasks);
}