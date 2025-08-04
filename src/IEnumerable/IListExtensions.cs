using RoeiBajayo.Infrastructure.Repositories.Files;
using System.Linq;
using System.Runtime.InteropServices;

namespace System.Collections.Generic;

public static class IListExtensions
{
    public static IEnumerable<T> AsReverse<T>(this IList<T> source)
    {
        for (var i = source.Count - 1; i >= 0; i--)
            yield return source[i];
    }

    public static IEnumerable<T> AsReverse<T>(this IList<T> source, int index) =>
        source.AsReverse(index, source.Count - index);

    public static IEnumerable<T> AsReverse<T>(this IList<T> source, int index, int count)
    {
        index--;
        for (var i = index + count; i >= index; i--)
        {
            yield return source[i];

            if (--count == 0)
                yield break;
        }
    }

    public static bool TryRemoveWhere<T>(this IList<T> source, Predicate<T> predicate)
    {
        for (int i = 0; i < source.Count; i++)
        {
            if (predicate(source[i]))
            {
                source.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    public static bool TryRemoveAllWhere<T>(this IList<T> source, Predicate<T> predicate)
    {
        var removed = false;
        for (int i = 0; i < source.Count; i++)
        {
            if (predicate(source[i]))
            {
                source.RemoveAt(i);
                removed = true;
            }
        }
        return removed;
    }

    public static int IndexOf<T>(this IList<T> collection, Func<T, bool> predicate, int index = 0, int? count = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if (index < 0 || index >= collection.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (count == 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        count ??= collection.Count - index;

        var stopAt = Math.Min(index + count.Value, collection.Count);

        var span = collection is T[] array ?
            array.AsSpan() :
            collection is List<T> list ?
            CollectionsMarshal.AsSpan(list) :
            collection is ArraySegment<T> segment ?
            segment.AsSpan() :
            [.. collection]; //todo: optimize

        for (var i = index; i < stopAt; i++)
        {
            if (predicate(span[i]))
            {
                return i;
            }
        }

        return -1;
    }
    public static int IndexOf<T>(this IList<T> collection, T item, int index = 0, int? count = null) =>
        IndexOf(collection, x => x?.Equals(item) ?? false, index, count);

    public static int LastIndexOf<T>(this IList<T> collection, Func<T, bool> predicate,
        int? index = null, int? count = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        if (index != null && (index <= 0 || index >= collection.Count))
            throw new ArgumentOutOfRangeException(nameof(index));

        if (count != null && (count == 0 || (index != null ? count > index : count > collection.Count)))
            throw new ArgumentOutOfRangeException(nameof(count));

        index ??= collection.Count - 1;

        var stopAt = count != null ? (index - count + 1) : 0;

        var span = collection is T[] array ?
            array.AsSpan() :
            collection is List<T> list ?
            CollectionsMarshal.AsSpan(list) :
            collection is ArraySegment<T> segment ?
            segment.AsSpan() :
            [.. collection]; //todo: optimize

        for (var i = index.Value; i >= stopAt; i--)
        {
            if (predicate(span[i]))
            {
                return i;
            }
        }

        return -1;
    }
    public static int LastIndexOf<T>(this IList<T> collection, T item, int index = 0, int? count = null) =>
        LastIndexOf(collection, x => x?.Equals(item) ?? false, index, count);

    public static void MoveIndex<T>(this List<T> collection, int fromIndex, int toIndex = 0)
    {
        if (fromIndex != toIndex)
        {
            collection.Insert(toIndex, collection[fromIndex]);
            collection.RemoveAt(toIndex < fromIndex ? fromIndex + 1 : fromIndex);
        }
    }

    public static List<T> MoveIndexWhere<T>(this IEnumerable<T> collection, Func<T, bool> whereExp, int toIndex = 0) =>
        MoveIndexWhere(collection is List<T> ? (collection as List<T>)! : collection.ToList(), whereExp, toIndex);
    public static List<T> MoveIndexWhere<T>(this List<T> collection, Func<T, bool> whereExp, int toIndex)
    {
        for (int i = 0; i < collection.Count; i++)
        {
            if (whereExp(collection[i]))
            {
                collection.MoveIndex(i, toIndex);
            }
        }
        return collection;
    }
    public static List<T> MoveToTop<T>(this IEnumerable<T> collection, Func<T, bool> whereExp) =>
        collection.MoveIndexWhere(whereExp, 0);
    public static List<T> MoveToBottom<T>(this IEnumerable<T> collection, Func<T, bool> whereExp) =>
        collection.MoveIndexWhere(whereExp, collection.Count() - 1);
    public static IList<T> Swap<T>(this IList<T> collection, int item1Index, int item2Index)
    {
        if (item1Index != item2Index)
        {
            (collection[item2Index], collection[item1Index]) = (collection[item1Index], collection[item2Index]);
        }
        return collection;
    }


    public static int TryLoad<T>(this IList<T> collection,
        string filename = "collection.json", string? path = null)
    {
        if (!new FileStorage<IList<T>>().TryLoad(filename, path, out IList<T>? loadedCollection))
            return 0;

        collection.Clear();
        for (var i = 0; i < loadedCollection!.Count; i++)
        {
            collection.Add(loadedCollection[i]);
        }
        return collection.Count;
    }
    public static IList<T> TryLoad<T>(string filename = "collection.json", string? path = null)
    {
        if (new FileStorage<IList<T>>().TryLoad(filename, path, out IList<T>? collection))
            return collection!;

        return [];
    }
    public static bool TryLoad<T>(string filename, out IList<T>? collection) =>
        TryLoad(filename, null, out collection);
    public static bool TryLoad<T>(string filename, string? path, out IList<T>? collection) =>
        new FileStorage<IList<T>>().TryLoad(filename, path, out collection);
}