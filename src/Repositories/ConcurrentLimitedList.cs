using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace RoeiBajayo.Infrastructure.Repositories;

/// <summary>
/// High-performance List that saves only the last added items. Thread-safe.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ConcurrentLimitedList<T>(int capacity, bool lazyInitialization = true) : LimitedList<T>(capacity)
{
    private ReaderWriterLockSlim? locker = lazyInitialization ? null : new ReaderWriterLockSlim();

    protected override void InternalAdd(T item)
    {
        locker ??= new();

        locker.EnterWriteLock();
        base.InternalAdd(item);
        locker.ExitWriteLock();
    }

    public override bool Remove(T item)
    {
        locker?.EnterWriteLock();
        var result = base.Remove(item);
        locker?.ExitWriteLock();
        return result;
    }

    public override void Clear()
    {
        locker?.EnterWriteLock();
        base.Clear();
        locker?.ExitWriteLock();
    }

    public new IEnumerator<T> GetEnumerator()
    {
        var items = new T[_capacity];
        locker?.EnterReadLock();
        var count = CopyTo(items);
        locker?.ExitReadLock();
        return new Enumerator(items, count);
    }

    private class Enumerator(T[] items, int count) : IEnumerator<T>
    {
        private int _index = -1;

        public T Current => items[_index];
        object IEnumerator.Current => items[_index]!;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return ++_index != count;
        }

        public void Reset()
        {
            _index = 0;
        }
    }

}