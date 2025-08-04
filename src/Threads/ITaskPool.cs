using System;
using System.Threading;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Threads;

public interface ITaskPool: IDisposable
{
    Task EnqueueAsync(Func<Task> task, CancellationToken cancellationToken);

    Task EnqueueAsync(Func<Task> task) =>
        EnqueueAsync(task, CancellationToken.None);
    void Enqueue(Func<Task> task) =>
        Enqueue(task, CancellationToken.None);
    void Enqueue(Func<Task> task, CancellationToken cancellationToken) =>
        _ = EnqueueAsync(task, cancellationToken);
}