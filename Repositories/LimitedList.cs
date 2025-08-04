using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Utils.Repositories;

/// <summary>
/// High-performance List that saves only the last added items
/// </summary>
/// <typeparam name="T"></typeparam>
public class LimitedList<T> : ICollection<T>
{
    internal readonly T[] _items;
    internal readonly int _capacity;

    private int _activeEnumerators = 0;
    private int _count = 0;
    private int _currentInsertIndex = 0;
    private int _firstIndex = 0;

    public LimitedList(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentException(null, nameof(capacity));

        _items = new T[capacity];
        _capacity = capacity;
    }

    public LimitedList(T[] collection)
    {
        if (collection.Length < 1)
            throw new ArgumentException(null, nameof(collection));

        _items = collection;
        _capacity = collection.Length;
    }

    public int Capacity => _items.Length;
    public int Count => _count;

    public bool IsReadOnly => false;

    public void Add(T item)
    {
        InternalAdd(item);
    }
    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            InternalAdd(item);
        }
    }
    protected virtual void InternalAdd(T item)
    {
        if (_activeEnumerators != 0)
            throw new InvalidOperationException("Cannot add items while enumerating");

        _items[_currentInsertIndex] = item;

        if (_count < _capacity)
        {
            _count++;
        }
        else
        {
            if (++_firstIndex == _capacity)
                _firstIndex = 0;
        }

        if (++_currentInsertIndex == _capacity)
            _currentInsertIndex = 0;
    }

    public virtual bool Remove(T item)
    {
        if (_activeEnumerators != 0)
            throw new InvalidOperationException("Cannot remove items while enumerating");

        var items = this.Where(x => !x!.Equals(item)).ToArray();
        if (items.Length == _count)
            return false;

        Clear();
        AddRange(items);
        return true;
    }

    public virtual void Clear()
    {
        if (_activeEnumerators != 0)
            throw new InvalidOperationException("Cannot clear items while enumerating");

        _count = 0;
        _currentInsertIndex = 0;
        _firstIndex = 0;
    }

    public T? LastOrDefault => _count == 0 ? default : _items[_currentInsertIndex == 0 ? _capacity - 1 : _currentInsertIndex - 1];

    public bool Contains(T item) =>
        this.Any(x => x!.Equals(item));

    public int CopyTo(T[] array)
    {
        CopyTo(array, 0, _count);
        return _count;
    }
    public void CopyTo(T[] array, int arrayIndex)
    {
        CopyTo(array, arrayIndex, _count);
    }
    public void CopyTo(T[] array, int arrayIndex, int count)
    {
        ArgumentNullException.ThrowIfNull(array);

        if (arrayIndex < 0 || (array.Length - arrayIndex) < _count)
            throw new IndexOutOfRangeException(nameof(arrayIndex));

        if (count < 0 || (array.Length - arrayIndex) < count || _count < count)
            throw new IndexOutOfRangeException(nameof(count));

        if (_count == 0)
            return;

        var span = _items.AsSpan();
        var result = array.AsSpan();
        var currentIndex = _firstIndex;
        var maxArrayIndex = arrayIndex + count;
        for (var i = arrayIndex; i < maxArrayIndex; i++)
        {
            result[i] = span[currentIndex];
            if (++currentIndex == _capacity)
                currentIndex = 0;
        }
    }

    public IEnumerator<T> GetEnumerator() =>
        new Enumerator(this);
    IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private class Enumerator : IEnumerator<T>
    {
        private readonly LimitedList<T> list;
        private int _currentIndex;
        private int _currentCount;

        public Enumerator(LimitedList<T> list)
        {
            this.list = list;
            _currentIndex = list._firstIndex;
            list._activeEnumerators++;
        }

        public T Current => list._items[_currentIndex];
        object IEnumerator.Current => list._items[_currentIndex]!;

        public void Reset()
        {
            _currentCount = 0;
            _currentIndex = list._firstIndex;
        }
        public bool MoveNext()
        {
            if (list._count == _currentCount)
                return false;

            if (_currentCount++ != 0 && ++_currentIndex == list._capacity)
            {
                _currentIndex = 0;
            }

            return true;
        }
        public void Dispose()
        {
            list._activeEnumerators--;
        }
    }
}