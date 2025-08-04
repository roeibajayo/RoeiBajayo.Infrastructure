using RoeiBajayo.Infrastructure.Threads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Repositories;

/// <summary>
/// Pool of generic T items, that allows you to recycle objects in order to avoid the overhead of creating and destroying them. 
/// The pool is designed to work with a specific type of object, which is specified by the type parameter T.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class Pool<T> where T : class
{
    private readonly int limit;
    private readonly TimeSpan instanceLifetime;
    private readonly Func<T> initializer;
    private readonly SemaphoreSlim semaphore;
    private readonly List<PoolItem<T>> active = [];
    private readonly Queue<PoolItem<T>> inactive = new();
    private readonly object locker = new();

    public Pool(int limit) : this(limit, TimeSpan.Zero) { }
    public Pool(int limit, TimeSpan instanceLifetime) : this(limit, instanceLifetime, Activator.CreateInstance<T>) { }
    public Pool(int limit, Func<T> initializer) : this(limit, TimeSpan.Zero, initializer) { }
    public Pool(int limit, TimeSpan instanceLifetime, Func<T> initializer)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(limit);

        this.limit = limit;
        this.instanceLifetime = instanceLifetime;
        this.initializer = initializer;
        semaphore = new SemaphoreSlim(limit);
    }

    public bool TryGet(out PoolItem<T>? item) =>
        TryGet(true, out item);
    private bool TryGet(bool wait, out PoolItem<T>? item)
    {
        Monitor.Enter(locker);
        try
        {
            var poolItem = GetInactiveItem();

            if (poolItem != null)
            {
                RecreateInstanceIfNeeded(poolItem);
                SetLastUse(poolItem);
                item = poolItem;
                return true;
            }

            // no reusable items found, try create
            if (active.Count < limit)
            {
                poolItem = CreatePoolItem()!;
                SetLastUse(poolItem);
                AddToActiveQueue(poolItem);

                if (wait)
                    semaphore.Wait();

                item = poolItem;
                return true;
            }


            item = default;
            return false;
        }
        finally
        {
            Monitor.Exit(locker);
        }
    }

    private void AddToActiveQueue(PoolItem<T> poolItem)
    {
        active.Add(poolItem);
    }
    private static void SetLastUse(PoolItem<T> useableItem)
    {
        useableItem.LastUse = DateTimeOffset.Now;
    }
    private void RecreateInstanceIfNeeded(PoolItem<T> useableItem)
    {
        if (instanceLifetime.Ticks != 0 &&
            useableItem.Lifetime > instanceLifetime)
        {
            // lifetime expired
            // try dispose expired instance
            if (useableItem.Instance is IDisposable disposable)
            {
                getNextRelease = null;
                try
                {
                    disposable.Dispose();
                }
                catch { }
            }

            useableItem.Created = DateTimeOffset.Now;
            useableItem.Instance = CreateInstance();
        }
    }
    private PoolItem<T> CreatePoolItem()
    {
        return new(this, CreateInstance());
    }
    private T CreateInstance()
    {
        return initializer();
    }
    private PoolItem<T>? GetInactiveItem()
    {
        // search for released item for reuse
        var isReuseItem = inactive.TryDequeue(out var useableItem);

        if (isReuseItem)
        {
            return useableItem;
        }

        // search for lifetime expired
        if (instanceLifetime.Ticks != 0 &&
            GetNextRelease() < DateTimeOffset.Now)
        {
            useableItem = active.FirstOrDefault(x => x.Lifetime > instanceLifetime);
            if (useableItem != null)
            {
                semaphore.Release();
            }
            return useableItem;
        }

        return null;
    }

    public async Task<PoolItem<T>?> TryGetOrWaitAsync() =>
        await TryGetOrWaitAsync(CancellationToken.None);
    public async Task<PoolItem<T>?> TryGetOrWaitAsync(CancellationToken cancellationToken)
    {
        try
        {
            PoolItem<T>? item;

            while (!TryGet(false, out item))
            {
                await WaitForNextLifetimeOrReleaseAsync(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }

            return item;
        }
        catch { }
        return default;
    }

    private async Task WaitForNextLifetimeOrReleaseAsync(CancellationToken cancellationToken)
    {
        CancellationTokenSource? delayCancellationToken = null;

        if (instanceLifetime.Ticks != 0)
        {
            var nextRelease = GetNextRelease();
            if (nextRelease is not null)
            {
                delayCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                AutoReleaseOnNextLifetime(nextRelease.Value, delayCancellationToken);
            }
        }

        await semaphore.WaitAsync(cancellationToken);

        delayCancellationToken?.Cancel();
    }
    private void AutoReleaseOnNextLifetime(DateTimeOffset nextRelease, CancellationTokenSource delayCancellationToken)
    {
        _ = Tasks.StartNow(async () =>
        {
            var until = nextRelease + TimeSpan.FromMilliseconds(10);
            await Tasks.DelayUntil(until, delayCancellationToken.Token);

            if (!delayCancellationToken.IsCancellationRequested)
                semaphore.Release();

        }, delayCancellationToken.Token);
    }

    private DateTimeOffset? getNextRelease;
    private DateTimeOffset? GetNextRelease()
    {
        if (getNextRelease == null &&
            instanceLifetime.Ticks != 0 &&
            active.Count == limit)
        {
            var nextRelease = active.MinBy(x => x.Created);
            getNextRelease = nextRelease == null ? null : nextRelease.Created + instanceLifetime;
        }
        return getNextRelease;
    }

    internal void Release(PoolItem<T> item)
    {
        Monitor.Enter(locker);
        try
        {
            if (item != null && (instanceLifetime.Ticks == 0 || item.Lifetime <= instanceLifetime))
            {
                inactive.Enqueue(item);
                semaphore.Release();
                getNextRelease = null;
            }
        }
        finally
        {
            Monitor.Exit(locker);
        }
    }
}

public sealed class PoolItem<T>(Pool<T> pool, T instance) : IDisposable where T : class
{
    public T? Instance { get; internal set; } = instance;
    public DateTimeOffset Created = DateTimeOffset.Now;
    internal DateTimeOffset LastUse;

    private bool disposed = false;

    public TimeSpan Lifetime => DateTimeOffset.UtcNow - Created;

    public void Release() =>
        Dispose();

    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        disposed = true;
        pool.Release(this);
    }
}
