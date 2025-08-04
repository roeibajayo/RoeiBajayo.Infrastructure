using RoeiBajayo.Infrastructure.Repositories.Files;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

namespace System.Linq;

public static class EnumerableExtensions
{
    public static DataTable ToDataTable<T>(this IEnumerable<T> collection) where T : class
    {
        var tb = new DataTable(typeof(T).Name);
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
            tb.Columns.Add(prop.Name, prop.PropertyType.BaseType!);

        foreach (var item in collection)
        {
            var values = new object[props.Length];
            for (var i = 0; i < props.Length; i++)
                values[i] = props[i].GetValue(item, null)!;
            tb.Rows.Add(values);
        }

        return tb;
    }

    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
            action(item);

        return collection;
    }
    public static IEnumerable<T> WhereIndex<T>(this IEnumerable<T> collection, Func<T, int, bool> selector)
    {
        var i = 0;
        foreach (var item in collection)
        {
            if (selector(item, i++))
                yield return item;
        }
    }

    public static ArraySegment<T> GetSegment<T>(this T[] arr, int offset, int? count = null)
    {
        count ??= arr.Length - offset;

        return new ArraySegment<T>(arr, offset, count.Value);
    }
    public static IEnumerable<ArraySegment<T>> GetSegments<T>(this T[] arr, int offset, int segmentSize)
    {
        if (offset < 0 || arr.Length < offset)
            throw new IndexOutOfRangeException(nameof(offset));

        for (; offset < arr.Length; offset += segmentSize)
        {
            if (offset + segmentSize > arr.Length)
                segmentSize = arr.Length - offset;

            yield return arr.GetSegment(offset, segmentSize);
        }
    }

    public static byte[] Combine(this IEnumerable<Array> arrs)
    {
        byte[] result = new byte[arrs.Sum(x => x.Length)];
        int index = 0;
        foreach (var arr in arrs)
        {
            Buffer.BlockCopy(arr, 0, result, index, arr.Length);
            index += arr.Length;
        }
        return result;
    }
    public static byte[] MemoryStreamCombine(this IEnumerable<byte[]> arrs)
    {
        using var memory = new MemoryStream();
        foreach (var arr in arrs)
            memory.Write(arr, 0, arr.Length);
        memory.Seek(0, SeekOrigin.Begin);
        return memory.ToArray();
    }

    public static bool TryCount<T>(this IEnumerable<T> collection, out int count)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection is ICollection<T> cT)
        {
            count = cT.Count;
            return true;
        }

        if (collection is ICollection c)
        {
            count = c.Count;
            return true;
        }

        if (collection is IReadOnlyCollection<T> roC)
        {
            count = roC.Count;
            return true;
        }

        count = 0;
        return false;
    }
    public static int CountImproved<T>(this IEnumerable<T> collection, int? takeCount = null)
    {
        var counted = collection.TryCount(out var count);
        return !counted ?
            (takeCount != null ? collection.Take(takeCount.Value) : collection).Count() :
            count;
    }

    public static void CopyToArray<T>(this IEnumerable<T> collection, int sourceIndex,
        T[] destinationArray, int destinationIndex, int length)
    {
        if (collection is Array sourceArray)
        {
            if (typeof(T) == typeof(byte))
            {
                Buffer.BlockCopy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
            }
            else
            {
                Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
            }
            return;
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(length, destinationArray.Length - destinationIndex);

        if (collection is IList<T> sourceList)
        {
            for (int i = 0; i < length; i++)
                destinationArray[destinationIndex + i] = sourceList[sourceIndex + i];

            return;
        }

        foreach (var item in collection.Skip(sourceIndex).Take(length))
            destinationArray[destinationIndex++] = item;
    }
    public static void CopyToArray(this byte[] source, int sourceIndex,
        byte[] destinationArray, int destinationIndex, int length)
    {
        Buffer.BlockCopy(source, sourceIndex, destinationArray, destinationIndex, length);
    }

    public static void Save<T>(this IEnumerable<T> collection, string filename = "collection.json", string? path = null) =>
        new FileStorage<IEnumerable<T>>().Save(collection ?? [], filename, path);
}