using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Threads;

public static partial class Tasks
{
    public static Task<T> StartWithTimeout<T>(Func<Task<T>> func, TimeSpan timeout) =>
        StartWithTimeout(func, timeout);
    public static Task<T> StartWithTimeout<T>(Task<T> func, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (timeout == TimeSpan.Zero)
            return func;

        using var cts = new CancellationTokenSource(timeout);
        return StartWithTimeout(func, cts.Token);
    }
    public static Task<T> StartWithTimeout<T>(Func<Task<T>> func, CancellationToken cancellationToken) =>
        StartWithTimeout(func(), cancellationToken);
    public static async Task<T> StartWithTimeout<T>(Task<T> func, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (cancellationToken == CancellationToken.None)
            return await func;

        var cancellationTaskCts = new CancellationTokenSource();
        var cancellationTask = Task.Run<T?>(async () =>
        {
            await Task.Delay(Timeout.Infinite, cancellationTaskCts.Token);
            return default;
        }, cancellationTaskCts.Token);
        var completed = await Task.WhenAny(func, cancellationTask!);
        cancellationTaskCts.Cancel();

        if (completed.Exception is not null)
            throw completed.Exception;

        if (completed.IsCanceled)
            throw new TaskCanceledException();

        return completed.Result;
    }

    public static Task StartWithTimeout(Func<Task> func, TimeSpan timeout) =>
        StartWithTimeout(func, timeout);
    public static Task StartWithTimeout(Task task, TimeSpan timeout)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (timeout == TimeSpan.Zero)
            return task;

        using var cts = new CancellationTokenSource(timeout);
        return StartWithTimeout(task, cts.Token);
    }
    public static Task StartWithTimeout(Func<Task> func, CancellationToken cancellationToken) =>
        StartWithTimeout(func(), cancellationToken);
    public static async Task StartWithTimeout(Task func, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(func);

        if (cancellationToken == CancellationToken.None)
        {
            await func;
            return;
        }

        var cancellationTaskCts = new CancellationTokenSource();
        var cancellationTask = Task.Run(async () =>
        {
            await Task.Delay(Timeout.Infinite, cancellationTaskCts.Token);
        }, cancellationTaskCts.Token);
        var completed = await Task.WhenAny(func, cancellationTask!);
        cancellationTaskCts.Cancel();

        if (completed.Exception is not null)
            throw completed.Exception;

        if (completed.IsCanceled)
            throw new TaskCanceledException();
    }
}