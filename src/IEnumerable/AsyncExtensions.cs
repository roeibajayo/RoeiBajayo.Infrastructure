using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Linq;

public static class AsyncExtensions
{
    public static async Task ForEachAsync<T>(this IEnumerable<T> collection, Func<T, Task> func,
        int degreeOfParallelism = 0)
    {
        if (degreeOfParallelism == 1)
        {
            foreach (var item in collection)
            {
                await func(item);
            }
            return;
        }

        if (degreeOfParallelism <= 0 ||
            degreeOfParallelism == int.MaxValue ||
            (collection.TryCount(out var count) && degreeOfParallelism >= count))
        {
            await Parallel.ForEachAsync(collection, async (item, _) => await func(item));
            return;
        }

        var options = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
        await Parallel.ForEachAsync(collection, options, async (item, _) => await func(item));
    }

    public static async Task<T[]> ToArrayAsync<T>(this IEnumerable<Task<T>> collection)
    {
        var results = new List<T>();
        foreach (var task in collection)
        {
            var result = await task;

            if (result == null || task.IsFaulted || task.IsCanceled)
                continue;

            results.Add(result);
        }
        return [.. results];
    }
    public static async Task<IList<TResult>> SelectAsync<T, TResult>(this IEnumerable<T> collection,
        Func<T, Task<TResult>> selector)
    {
        var results = new List<TResult>();
        foreach (var item in collection)
        {
            var task = selector(item);
            var result = await task;

            if (result == null || task.IsFaulted || task.IsCanceled)
                continue;

            results.Add(result);
        }
        return results;
    }
    public static async Task<IEnumerable<TResult>> SelectManyAsync<T, TResult>(this IEnumerable<T> collection,
        Func<T, Task<IEnumerable<TResult>>> selector)
    {
        var results = new List<TResult>();
        foreach (var item in collection)
        {
            foreach (var result in await selector(item))
            {
                if (result != null)
                    results.Add(result);
            }
        }
        return results;
    }
    public static async Task<IEnumerable<T>> SelectManyAsync<T>(this IEnumerable<Task<IEnumerable<T>>> collection)
    {
        var results = new List<T>();
        foreach (var task in collection)
        {
            foreach (var result in await task)
            {
                if (result != null)
                    results.Add(result);
            }
        }
        return results;
    }

    public static async Task<T[]> ToArrayParallelAsync<T>(this IEnumerable<Task<T>> collection,
        int degreeOfParallelism = 0)
    {
        var results = new ConcurrentBag<T>();
        await collection.ForEachAsync(async task =>
        {
            var result = await task;

            if (result == null || task.IsFaulted || task.IsCanceled)
                return;

            results.Add(result);
        }, degreeOfParallelism);
        return [.. results];
    }
    public static async Task<TResult[]> SelectParallelAsync<T, TResult>(this IEnumerable<T> collection,
        Func<T, Task<TResult>> selector,
        int degreeOfParallelism = 0)
    {
        var results = new ConcurrentBag<TResult>();
        await collection.ForEachAsync(async item =>
        {
            var task = selector(item);
            var result = await task;

            if (result == null || task.IsFaulted || task.IsCanceled)
                return;

            results.Add(result);
        }, degreeOfParallelism);
        return [.. results];
    }
    public static async Task<IEnumerable<TResult>> SelectManyParallelAsync<T, TResult>(this IEnumerable<T> collection,
        Func<T, Task<IEnumerable<TResult>>> selector,
        int degreeOfParallelism = 0)
    {
        var results = new ConcurrentBag<TResult>();
        await collection.ForEachAsync(async task =>
        {
            foreach (var result in await selector(task))
            {
                if (result != null)
                    results.Add(result);
            }
        }, degreeOfParallelism);
        return results;
    }
    public static async Task<IEnumerable<TResult>> SelectManyParallelAsync<T, TResult>(this IEnumerable<IEnumerable<T>> collection,
        Func<IEnumerable<T>, Task<IEnumerable<TResult>>> selector,
        int degreeOfParallelism = 0)
    {
        var results = new ConcurrentBag<TResult>();
        await collection.ForEachAsync(async task =>
        {
            foreach (var result in await selector(task))
            {
                if (result != null)
                    results.Add(result);
            }
        }, degreeOfParallelism);
        return results;
    }
    public static async Task<IEnumerable<T>> SelectManyParallelAsync<T>(this IEnumerable<Task<IEnumerable<T>>> collection,
        int degreeOfParallelism = 0)
    {
        var results = new ConcurrentBag<T>();
        await collection.ForEachAsync(async task =>
        {
            foreach (var result in await task)
            {
                if (result != null)
                    results.Add(result);
            }
        }, degreeOfParallelism);
        return results;
    }
}