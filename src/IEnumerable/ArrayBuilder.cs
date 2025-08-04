using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Infrastructure.Utils.IEnumerable;

public class ArrayBuilder<T>
{
    internal readonly List<ICollection<T>> _collection = [];
    internal bool _lastIsInsertable;

    public ArrayBuilder() { }
    public ArrayBuilder(IEnumerable<ICollection<T>> arrays) => AddRange(arrays);

    public void Add(T item)
    {
        if (_lastIsInsertable && _collection[^1] is List<T> list)
        {
            list.Add(item);
            return;
        }

        Add(new List<T>([item]));
        _lastIsInsertable = true;
    }
    public void Add(ICollection<T> arr)
    {
        _collection.Add(arr);
        _lastIsInsertable = false;
    }
    public void AddRange(IEnumerable<ICollection<T>> arrays)
    {
        foreach (var arr in arrays)
            Add(arr);

        _lastIsInsertable = false;
    }

    public IEnumerable<T[]> SplitEvery(int length, int offset = 0, int count = 0)
    {
        if (count == 0)
            count = Count - offset;

        int index = 0, ix = 0, srcOffest = 0;
        var buffer = ArrayPool<T>.Shared.Rent(length);

        try
        {
            for (var i = offset; i < offset + count; i += length)
            {
                if (i + length > offset + count)
                {
                    length = offset + count - i;
                }

                var read = GetSegment(buffer, i, length, ref index, ref ix, ref srcOffest);
                var splitItem = new T[read];
                Array.Copy(buffer, 0, splitItem, 0, read);
                yield return splitItem;
            }
        }
        finally
        {
            ArrayPool<T>.Shared.Return(buffer);
        }
    }

    public int GetSegment(T[] buffer, int offset, int count)
    {
        int index = 0, i = 0, srcOffest = 0;
        return GetSegment(buffer, offset, count, ref index, ref i, ref srcOffest);
    }
    internal int GetSegment(T[] buffer, int offset, int count,
        ref int index, ref int i, ref int srcOffset)
    {
        int collectionLength;
        int read = 0;

        if (_collection.Count <= i)
            return 0;

        if (i == 0 && index == 0)
        {
            collectionLength = _collection[i].Count;

            while (index + collectionLength < offset)
            {
                index += collectionLength;

                if (_collection.Count == ++i)
                    break;

                collectionLength = _collection[i].Count;
            }

            srcOffset = offset - index;
            if (srcOffset < 0) srcOffset = 0;
        }

        collectionLength = _collection[i].Count;
        int dstOffest = 0;
        while (dstOffest != count)
        {
            int length = count - dstOffest;

            if (length > collectionLength - srcOffset)
                length = collectionLength - srcOffset;

            read += length;

            _collection[i].CopyToArray(srcOffset, buffer, dstOffest, length);

            dstOffest += length;
            index += length;

            if (srcOffset + length >= collectionLength)
            {
                if (_collection.Count == ++i)
                    break;

                collectionLength = _collection[i].Count;
                srcOffset = 0;
            }
            else
            {
                srcOffset += length;
            }
        }

        return read;
    }

    public IEnumerable<T[]> GetSegments(T[] buffer, int bufferSize, int offset = 0, int count = 0)
    {
        int initOffest = offset;
        int index = 0;
        int iCollection = 0;
        int srcOffest = 0;
        int length = Count;
        bool completed = false;

        if (count == 0 || count > length - offset)
            count = length - offset;

        while (!completed)
        {
            if (offset + bufferSize > initOffest + count)
            {
                bufferSize = initOffest + count - offset;
                if (bufferSize == 0)
                    break;
                completed = true;
            }

            var read = GetSegment(buffer, offset, bufferSize, ref index, ref iCollection, ref srcOffest);
            var splitItem = new T[read];
            Array.Copy(buffer, 0, splitItem, 0, read);

            yield return splitItem;
            offset += bufferSize;
        }
    }

    public int Count => _collection.Sum(x => x.Count);
    public IEnumerable<T> AsCombined() => _collection.SelectMany(x => x);
    public void Clear() => _collection.Clear();
    public bool Contains(T item) => _collection.Any(x => x.Contains(item));
    public T[] ToArray() => AsCombined().ToArray();
}

public static class ArrayBuilderExtensions
{
    public static void WriteTo(this ArrayBuilder<byte> combiner, Stream stream, int bufferSize = 4096)
    {
        //var bufferStream = new BufferedStream(stream, bufferSize);
        //foreach (var arr in combiner)
        //    bufferStream.Write((arr as byte[]) ?? arr.ToArray());
        //bufferStream.Flush();
        //return;

        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            int read;
            int offset = 0;
            int index = 0;
            int i = 0;
            int srcOffest = 0;
            while ((read = combiner.GetSegment(buffer, offset, bufferSize, ref index, ref i, ref srcOffest)) != 0)
            {
                stream.Write(buffer, 0, read);
                offset += read;
            }
            stream.Flush();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    public static byte[] Combine(this ArrayBuilder<byte> combiner)
    {
        unchecked
        {
            return combiner._collection.Cast<byte[]>().Combine();
        }
    }
}
