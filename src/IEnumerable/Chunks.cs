using System.Collections.Generic;

namespace System.Linq;

public static class ChunksExtensions
{
    public static int Chunk<T>(this IEnumerable<T> collection, T[] buffer,
        int size, int chunkIndex = 0, int offset = 0)
    {
        if (size < 1)
            throw new ArgumentException(null, nameof(size));

        ArgumentOutOfRangeException.ThrowIfNegative(chunkIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentNullException.ThrowIfNull(collection);

        if (chunkIndex == 0 && buffer.Length < size)
            throw new ArgumentOutOfRangeException(nameof(buffer));

        var from = offset + (chunkIndex * size);

        if (collection is T[] asArray)
        {
            if (size > asArray.Length - from)
                size = asArray.Length - from;

            if (size < 0)
                return 0;

            Array.Copy(asArray, from, buffer, 0, size);
            return size;
        }

        if (collection is IList<T> asList)
        {
            var max = offset + (chunkIndex + 1) * size;

            if (max > asList.Count)
                max = asList.Count;

            if (from > max)
                return 0;

            var i = 0;
            var count = max - from;
            for (; from < max; from++)
                buffer[i++] = asList[from];

            return count;
        }

        if (collection is IQueryable<T> asQuery)
        {
            collection = asQuery.Skip(from).Take(size);
            from = 0;
        }

        var enumerator = collection.GetEnumerator();
        try
        {
            return InternalReadChunk(buffer, size, ref from, ref enumerator);
        }
        finally
        {
            enumerator.Dispose();
        }
    }
    public static IEnumerable<T> Chunk<T>(this IEnumerable<T> collection,
        int size, int chunkIndex = 0, int offset = 0)
    {
        if (size < 1)
            throw new ArgumentException(null, nameof(size));

        ArgumentOutOfRangeException.ThrowIfNegative(chunkIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentNullException.ThrowIfNull(collection);

        var from = offset + (chunkIndex * size);

        if (collection is T[] asArray)
        {
            int max = (chunkIndex + 1) * size;

            if (max > asArray.Length)
                max = asArray.Length;

            if (from > max)
                yield break;

            for (; from < max; from++)
                yield return asArray[from];

            yield break;
        }

        if (collection is IList<T> asList)
        {
            var max = (chunkIndex + 1) * size;

            if (max > asList.Count)
                max = asList.Count;

            if (from > max)
                yield break;

            for (; from < max; from++)
                yield return asList[from];

            yield break;
        }

        foreach (var item in collection.Skip(from).Take(size))
            yield return item;

    }
    public static IEnumerable<T[]> Chunks<T>(this IEnumerable<T> collection, int size, int offset = 0)
    {
        if (size < 1)
            throw new ArgumentException(null, nameof(size));

        ArgumentNullException.ThrowIfNull(collection);

        var chunkIndex = 0;
        if (collection is IQueryable<T> asQuery)
        {
            asQuery = asQuery.Skip(offset);
            T[] chunk = [.. asQuery.Skip(chunkIndex * size).Take(size)];
            while (chunk.Length != 0)
            {
                yield return chunk;
                chunkIndex++;
                chunk = [.. asQuery.Skip(chunkIndex * size).Take(size)];
            }
            yield break;
        }

        var buffer = new T[size];
        int read;

        if (collection is ICollection<T>)
        {
            while ((read = collection.Chunk(buffer, size, chunkIndex++, offset)) != 0)
            {
                if (read != size)
                    Array.Resize(ref buffer, read);

                yield return buffer;
            }
        }
        else
        {
            var from = offset + (chunkIndex * size);
            var e = collection.GetEnumerator();
            try
            {
                while ((read = InternalReadChunk(buffer, size, ref from, ref e)) != 0)
                {
                    if (read != size)
                        Array.Resize(ref buffer, read);

                    yield return buffer;
                    from = 0;
                }
            }
            finally
            {
                e.Dispose();
            }
        }
    }
    private static int InternalReadChunk<T>(T[] buffer, int size,
        ref int from, ref IEnumerator<T> e)
    {
        var count = 0;
        for (var i = 0; i < from; i++)
        {
            if (!e.MoveNext()) return count;
        }
        for (var i = 0; i < size; i++)
        {
            if (!e.MoveNext()) return count;
            buffer[i] = e.Current;
            count++;
        }
        return count;
    }

}
