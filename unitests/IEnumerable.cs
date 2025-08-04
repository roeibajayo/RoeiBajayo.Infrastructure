using RoeiBajayo.Infrastructure.IEnumerable;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace UnitTestProject;

public class IEnumerable
{

    [Fact]
    public void GroupByRanges()
    {
        var range1Size = 2;
        var range2Size = 3;
        var span = 1;
        var range3Size = 2;
        var range1 = new int[range1Size];
        var range2 = new int[range2Size];
        var range3 = new int[range3Size];

        for (var i = 0; i < range1Size; i++)
        {
            range1[i] = i;
        }
        for (var i = 0; i < range2Size; i++)
        {
            range2[i] = range1Size + i;
        }
        for (var i = 0; i < range3Size; i++)
        {
            range3[i] = range1Size + range2Size + span + i;
        }

        var list = range1.Concat(range2).Concat(range3);
        var groups = Ranges.Group(list);

        Assert.True(groups.Count == 2);
        Assert.True(groups[0].Start.Value == 0);
        Assert.True(groups[0].End.Value == (range1Size + range2Size - 1));
        Assert.True(groups[1].Start.Value == range1Size + range2Size + span);
        Assert.True(groups[1].End.Value == (range1Size + range2Size + span + range3Size - 1));
    }

    [Fact]
    public void ArrayBuilder_SplitEvery()
    {

        var list = new List<int[]>
        {
            new [] { 0,1,2},
            new [] { 3,4,5},
            new [] { 6,7,8}
        };
        var combiner = new ArrayBuilder<int>(list);
        var splited = combiner.SplitEvery(2).ToList();

        Assert.True(splited.Count == 5);
        Assert.True(splited[0].Length == 2);
        Assert.True(splited[4].Length == 1);
        Assert.True(splited[0][0] == 0);
        Assert.True(splited[0][1] == 1);
        Assert.True(splited[3][0] == 6);
        Assert.True(splited[3][1] == 7);
        Assert.True(splited[4][0] == 8);

        splited = combiner.SplitEvery(3, 2, 5).ToList();
        Assert.True(splited.Count == 2);
        Assert.True(splited[0].Length == 3);
        Assert.True(splited[1].Length == 2);
        Assert.True(splited[0][0] == 2);
        Assert.True(splited[0][1] == 3);
        Assert.True(splited[0][2] == 4);
        Assert.True(splited[1][0] == 5);
        Assert.True(splited[1][1] == 6);

        splited = combiner.SplitEvery(2, 2, 5).ToList();
        Assert.True(splited.Count == 3);
        Assert.True(splited[0].Length == 2);
        Assert.True(splited[1].Length == 2);
        Assert.True(splited[2].Length == 1);
        Assert.True(splited[0][0] == 2);
        Assert.True(splited[0][1] == 3);
        Assert.True(splited[1][0] == 4);
        Assert.True(splited[1][1] == 5);
        Assert.True(splited[2][0] == 6);


        splited = combiner.SplitEvery(2, 2, 6).ToList();
        Assert.True(splited.Count == 3);
        Assert.True(splited[0].Length == 2);
        Assert.True(splited[1].Length == 2);
        Assert.True(splited[2].Length == 2);
        Assert.True(splited[0][0] == 2);
        Assert.True(splited[0][1] == 3);
        Assert.True(splited[1][0] == 4);
        Assert.True(splited[1][1] == 5);
        Assert.True(splited[2][0] == 6);
        Assert.True(splited[2][1] == 7);

    }

    [Fact]
    public void ArrayBuilder_GetSegment()
    {
        var list = new List<int[]>
        {
            new [] { 0,1,2},
            new [] { 3,4,5},
            new [] { 6,7,8}
        };

        var combiner = new ArrayBuilder<int>(list);
        var buffer = new int[4];

        var segment = combiner.GetSegment(buffer, 0, 4);
        Assert.Equal(4, segment);
        Assert.Equal(0, buffer[0]);
        Assert.Equal(1, buffer[1]);
        Assert.Equal(2, buffer[2]);
        Assert.Equal(3, buffer[3]);

        segment = combiner.GetSegment(buffer, 1, 4);
        Assert.Equal(4, segment);
        Assert.Equal(1, buffer[0]);
        Assert.Equal(2, buffer[1]);
        Assert.Equal(3, buffer[2]);
        Assert.Equal(4, buffer[3]);

        segment = combiner.GetSegment(buffer, 3, 3);
        Assert.Equal(3, segment);
        Assert.Equal(3, buffer[0]);
        Assert.Equal(4, buffer[1]);
        Assert.Equal(5, buffer[2]);
    }

    [Fact]
    public void ArrayBuilder_GetBufferCollection()
    {

        var list = new List<int[]>
        {
            new [] { 0,1,2},
            new [] { 3,4,5},
            new [] { 6,7,8}
        };
        var buffer = new int[2];

        var combiner = new ArrayBuilder<int>(list);
        var bufferCollection = combiner.GetSegments(buffer, 2);
        var buffers = bufferCollection.ToArray();

        Assert.True(buffers.Length == 5);
        Assert.True(buffers[0].Length == 2);
        Assert.True(buffers[0][1] == 1);
        Assert.True(buffers[1].Length == 2);
        Assert.True(buffers[2].Length == 2);
        Assert.True(buffers[3].Length == 2);
        Assert.True(buffers[4].Length == 1);
        Assert.True(buffers[4][0] == 8);


        bufferCollection = combiner.GetSegments(buffer, 2, 1, 2);
        buffers = bufferCollection.ToArray();

        Assert.True(buffers.Length == 1);
        Assert.True(buffers[0].Length == 2);
        Assert.True(buffers[0][0] == 1);
        Assert.True(buffers[0][1] == 2);

        bufferCollection = combiner.GetSegments(buffer, 2, 1, 6);
        buffers = bufferCollection.ToArray();

        Assert.True(buffers.Length == 3);
        Assert.True(buffers[0].Length == 2);
        Assert.True(buffers[0][0] == 1);
        Assert.True(buffers[0][1] == 2);
        Assert.True(buffers[2].Length == 2);
        Assert.True(buffers[2][1] == 6);
    }

    [Fact]
    public void ArrayBuilder_WriteAllBytes()
    {
        var count = 500;
        var maxBytes = 60;
        var list = new List<byte[]>(count);
        unchecked
        {
            for (var i = 0; i != count; i++)
            {
                if (--maxBytes == 0)
                    maxBytes = 6000;
                var innerList = new byte[maxBytes];
                for (var j = 0; j != maxBytes; j++)
                {
                    innerList[j] = (byte)(j % 2);
                }
                list.Add(innerList);
            }
        }
        var expected = list.Sum(x => x.Length);

        using var stream = new MemoryStream();
        stream.WriteAllBytes(list);
        Assert.Equal(expected, stream.Length);
    }

    [Fact]
    public void Chunks()
    {

        IEnumerable<int> list = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        using (var e = list.Chunks(3).GetEnumerator())
        {
            Assert.True(e.MoveNext());
            Assert.Equal(1, e.Current[0]);
            Assert.Equal(2, e.Current[1]);
            Assert.Equal(3, e.Current[2]);
            Assert.Equal(3, e.Current.Length);
            Assert.True(e.MoveNext());
            Assert.Equal(4, e.Current[0]);
            Assert.Equal(5, e.Current[1]);
            Assert.Equal(6, e.Current[2]);
            Assert.Equal(3, e.Current.Length);
            Assert.True(e.MoveNext());
            Assert.Equal(7, e.Current[0]);
            Assert.Equal(8, e.Current[1]);
            Assert.Equal(9, e.Current[2]);
            Assert.Equal(3, e.Current.Length);
            Assert.True(e.MoveNext());
            Assert.Equal(10, e.Current[0]);
            Assert.Single(e.Current);
            Assert.False(e.MoveNext());
        }

        list = [1, 2, 3, 4, 5, 6, 7, 8, 9];
        using (var e = list.Chunks(3).GetEnumerator())
        {
            Assert.True(e.MoveNext());
            Assert.Equal(1, e.Current[0]);
            Assert.Equal(2, e.Current[1]);
            Assert.Equal(3, e.Current[2]);
            Assert.Equal(3, e.Current.Length);
            Assert.True(e.MoveNext());
            Assert.Equal(4, e.Current[0]);
            Assert.Equal(5, e.Current[1]);
            Assert.Equal(6, e.Current[2]);
            Assert.Equal(3, e.Current.Length);
            Assert.True(e.MoveNext());
            Assert.Equal(7, e.Current[0]);
            Assert.Equal(8, e.Current[1]);
            Assert.Equal(9, e.Current[2]);
            Assert.Equal(3, e.Current.Length);
            Assert.False(e.MoveNext());
        }

        list = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.ToList();
        using (var e = list.Chunks(3).GetEnumerator())
        {
            Assert.True(e.MoveNext());
            Assert.Equal(1, e.Current[0]);
            Assert.Equal(2, e.Current[1]);
            Assert.Equal(3, e.Current[2]);
            Assert.Equal(3, e.Current.Length);
            Assert.True(e.MoveNext());
            Assert.Equal(4, e.Current[0]);
            Assert.Equal(5, e.Current[1]);
            Assert.Equal(6, e.Current[2]);
            Assert.Equal(3, e.Current.Length);
            Assert.True(e.MoveNext());
            Assert.Equal(7, e.Current[0]);
            Assert.Equal(8, e.Current[1]);
            Assert.Equal(9, e.Current[2]);
            Assert.Equal(3, e.Current.Length);
            Assert.True(e.MoveNext());
            Assert.Equal(10, e.Current[0]);
            Assert.Single(e.Current);
            Assert.False(e.MoveNext());
        }

        list = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.Where(x => true);//.AsEnumerable();
        using (var e = list.Chunks(3).GetEnumerator())
        {
            Assert.True(e.MoveNext());
            Assert.Equal(1, e.Current[0]);
            Assert.Equal(2, e.Current[1]);
            Assert.Equal(3, e.Current[2]);
            Assert.Equal(3, e.Current.Length);
            Assert.True(e.MoveNext());
            Assert.Equal(4, e.Current[0]);
            Assert.Equal(5, e.Current[1]);
            Assert.Equal(6, e.Current[2]);
            Assert.Equal(3, e.Current.Length);
            Assert.True(e.MoveNext());
            Assert.Equal(7, e.Current[0]);
            Assert.Equal(8, e.Current[1]);
            Assert.Equal(9, e.Current[2]);
            Assert.Equal(3, e.Current.Length);
            Assert.True(e.MoveNext());
            Assert.Equal(10, e.Current[0]);
            Assert.Single(e.Current);
            Assert.False(e.MoveNext());
        }

        list = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        var chunk = list.Chunk(4, 0).ToArray();
        Assert.Equal(1, chunk[0]);
        Assert.Equal(2, chunk[1]);
        Assert.Equal(3, chunk[2]);
        Assert.Equal(3, chunk[2]);

        list = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        chunk = list.Chunk(4, 0).ToArray();
        Assert.Equal(1, chunk[0]);
        Assert.Equal(2, chunk[1]);
        Assert.Equal(3, chunk[2]);
        Assert.Equal(3, chunk[2]);

        list = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }.Where(x => true);//.AsEnumerable();
        chunk = list.Chunk(4, 0).ToArray();
        Assert.Equal(1, chunk[0]);
        Assert.Equal(2, chunk[1]);
        Assert.Equal(3, chunk[2]);
        Assert.Equal(3, chunk[2]);

        list = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        using (var e = list.Chunks(3, 8).GetEnumerator())
        {
            Assert.True(e.MoveNext());
            Assert.Equal(9, e.Current[0]);
            Assert.Equal(10, e.Current[1]);
            Assert.Equal(2, e.Current.Length);
            Assert.False(e.MoveNext());
        }

        list = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        using (var e = list.Chunks(2, 7).GetEnumerator())
        {
            Assert.True(e.MoveNext());
            Assert.Equal(8, e.Current[0]);
            Assert.Equal(9, e.Current[1]);
            Assert.Equal(2, e.Current.Length);
            Assert.True(e.MoveNext());
            Assert.Equal(10, e.Current[0]);
            Assert.Single(e.Current);
            Assert.False(e.MoveNext());
        }
    }


    [Fact]
    public async Task ForEachAsync()
    {
        var count = 10;
        var actual = new BlockingCollection<int>();
        var list = Enumerable.Range(1, count);
        var timestamp = DateTime.Now;
        await list.ForEachAsync(async x =>
        {
            await Task.Delay(100);
            actual.Add(0);
        });
        var elapsed = DateTime.Now - timestamp;

        Assert.Equal(count, actual.Count);
        Assert.True(elapsed.TotalMilliseconds < 150);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task ForEachAsync_WithDegree(int degreeOfParallelism)
    {
        var delay = 100;
        var count = 10;
        var actual = new BlockingCollection<int>();
        var list = Enumerable.Range(1, count);
        var timestamp = new Stopwatch();
        timestamp.Start();
        await list.ForEachAsync(async x =>
        {
            await Task.Delay(delay);
            actual.Add(0);
        }, degreeOfParallelism);
        timestamp.Stop();

        var expected = delay * count / degreeOfParallelism;
        Assert.Equal(count, actual.Count);
        Assert.InRange(timestamp.ElapsedMilliseconds, expected, expected + 150);
    }

}
