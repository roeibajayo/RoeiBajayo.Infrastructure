using Infrastructure.Utils.Repositories;
using Infrastructure.Utils.Repositories.Database;
using Infrastructure.Utils.Repositories.Persistent;
using Infrastructure.Utils.Repositories.Queues;
using Infrastructure.Utils.Repositories.Queues.Throttling;
using Infrastructure.Utils.Repositories.Queues.Throttling.Models;
using Xunit;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTestProject;


public class Repositories
{
    [Fact]
    public async Task ThrottlingCounter()
    {
        var waitingTime = 2;
        var queue = new ThrottlingCounter([
            new ThrottlingTimeSpan(TimeSpan.FromSeconds(waitingTime), 2)
        ]);
        Assert.True(queue.TryEnqueue());
        Assert.True(queue.TryEnqueue());
        var stopwatcher = new Stopwatch();
        stopwatcher.Start();
        await queue.WaitForEnqueueAsync(CancellationToken.None);
        await queue.WaitForEnqueueAsync(CancellationToken.None);
        stopwatcher.Stop();
        Console.WriteLine("waited " + stopwatcher.ElapsedMilliseconds + "ms");
        Assert.True(stopwatcher.ElapsedMilliseconds >= (waitingTime * 1000 - 50) &&
            stopwatcher.ElapsedMilliseconds <= (waitingTime * 1000 + 50));


        queue = new ThrottlingCounter([
            new ThrottlingTimeSpan(TimeSpan.FromSeconds(1), 2)
        ]);
        Assert.True(queue.TryEnqueue());
        await Task.Delay(500);
        Assert.True(queue.TryEnqueue());
        Assert.False(queue.TryEnqueue());
        await Task.Delay(501);
        Assert.True(queue.TryEnqueue());
        Assert.False(queue.TryEnqueue());
        await Task.Delay(501);
        Assert.True(queue.TryEnqueue());
        Assert.False(queue.TryEnqueue());
        await Task.Delay(1001);
        Assert.True(queue.TryEnqueue());
        Assert.True(queue.TryEnqueue());
        Assert.False(queue.TryEnqueue());
    }

    [Fact]
    public async Task ThrottlingQueue()
    {
        var queue = new ThrottlingQueue<int>([
            new ThrottlingTimeSpan(TimeSpan.FromSeconds(1) ,2)
        ]);
        queue.Enqueue([1, 2, 3, 4, 5]);

        var items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Equal(2, items.Length);
        Assert.Equal(1, items[0]);
        Assert.Equal(2, items[1]);

        await Task.Delay(500);

        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Empty(items);

        await Task.Delay(600);

        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Equal(2, items.Length);
        Assert.Equal(3, items[0]);
        Assert.Equal(4, items[1]);

        await Task.Delay(1001);

        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Single(items);
        Assert.Equal(5, items[0]);

        await Task.Delay(1001);

        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Empty(items);


        queue = new ThrottlingQueue<int>([
            new ThrottlingTimeSpan(TimeSpan.FromHours(1), 10, true)
        ]);
        queue.Enqueue([1, 2, 3, 4, 5]);
        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Equal(5, items.Length);
        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Empty(items);
        queue.Enqueue([1, 2, 3, 4, 5, 6]);
        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Equal(5, items.Length);
        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Empty(items);



        queue = new ThrottlingQueue<int>([
            new ThrottlingTimeSpan(TimeSpan.FromSeconds(3), 2),
            new ThrottlingTimeSpan(TimeSpan.FromSeconds(1), 1)
        ]);
        queue.Enqueue([1, 2, 3, 4, 5]);

        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Single(items);

        await Task.Delay(1001);

        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Single(items);

        await Task.Delay(1001);

        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Empty(items);

        await Task.Delay(1001);

        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Single(items);

        await Task.Delay(1001);

        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Single(items);

        await Task.Delay(1001);

        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Empty(items);

        await Task.Delay(1001);

        items = (await queue.TryDequeueAsync()).ToArray();
        Assert.Single(items);
    }

    [Fact]
    public void LimitedList_General()
    {
        var mylist = new LimitedList<int>(3)
        {
            1, 2, 3, 4, 5
        };
        var list = mylist.ToArray();

        Assert.Equal(3, list[0]);
        Assert.Equal(4, list[1]);
        Assert.Equal(5, list[2]);

        Assert.Equal(0, Array.IndexOf(list, 3));
        Assert.Equal(1, Array.IndexOf(list, 4));
        Assert.Equal(2, Array.IndexOf(list, 5));
        Assert.Equal(-1, Array.IndexOf(list, 98));

        mylist.Add(98);
        list = [.. mylist];

        Assert.Equal(4, list[0]);
        Assert.Equal(5, list[1]);
        Assert.Equal(98, list[2]);

        mylist = new LimitedList<int>(3);
        mylist.AddRange(
        [
            1,2,3,4,5,6
        ]);
        list = [.. mylist];
        Assert.Equal(4, list[0]);
        Assert.Equal(5, list[1]);
        Assert.Equal(6, list[2]);

        mylist.Clear();
        list = [.. mylist];
        Assert.Empty(list);

        mylist = new LimitedList<int>(3)
        {
            1, 2, 3
        };

        var itemContent = 1;
        foreach (var item in mylist)
        {
            Assert.Equal(item, itemContent++);
        }

        mylist = new LimitedList<int>(3)
        {
            1, 2, 3, 4, 5
        };
        itemContent = 3;
        foreach (var item in mylist)
        {
            Assert.Equal(item, itemContent++);
        }
    }

    [Fact]
    public void LimitedList_AddMassive()
    {
        var count = 100007;
        var capacity = 10002;
        var list = new LimitedList<int>(capacity);

        for (var i = 1; i <= count; i++)
        {
            list.Add(i);
        }

        var index = 1;
        foreach (var i in list)
        {
            Assert.Equal(i, count - capacity + index++);
        }
    }

    [Fact]
    public void LimitedList_AddRangeMassive()
    {
        var count = 100007;
        var capacity = 10002;
        var list = new LimitedList<int>(capacity);
        list.AddRange(Enumerable.Range(0, 100007));

        var index = 0;
        foreach (var i in list)
        {
            Assert.Equal(i, count - capacity + index++);
        }

        list.Add(0);
        Assert.True(list.Last() == 0);

        list.Add(1);
        Assert.True(list.Last() == 1);

    }

    [Fact]
    public void AccumulatorQueue()
    {
        using var are = new AutoResetEvent(false);
        var count = 10;
        var tries = 2;

        var acc = new AccumulatorQueue<int>((items) =>
        {
            if (items.Count() == count / tries)
                are.Set();
        }, maxItemsPerBulk: count / tries, interval: 150);


        for (var i = 0; i < count; i++)
        {
            acc.Enqueue(i);
        }

        for (var i = 0; i < tries; i++)
        {
            Assert.True(are.WaitOne(timeout: TimeSpan.FromSeconds(0.5)));
        }
    }

    public record PoolItem(int Value);
    [Fact]
    public void Pool_Regular()
    {
        int i = 0;
        var pool = new Pool<PoolItem>(2, () => new(++i));
        Assert.True(pool.TryGet(out var a));
        Assert.Equal(1, a.Instance.Value);
        Assert.True(pool.TryGet(out var b));
        Assert.Equal(2, b.Instance.Value);
        Assert.False(pool.TryGet(out var c));

        a.Release();

        Assert.True(pool.TryGet(out var d));
        Assert.Equal(a.Instance.Value, d.Instance.Value);
        Assert.False(pool.TryGet(out var e));
    }
    [Fact]
    public async Task Pool_Lifetime()
    {
        int i = 0;
        var pool = new Pool<PoolItem>(2, TimeSpan.FromMilliseconds(1000), () => new(++i));

        Assert.True(pool.TryGet(out var a));
        Assert.Equal(1, a.Instance.Value);

        Assert.True(pool.TryGet(out var b));
        Assert.Equal(2, b.Instance.Value);

        Assert.False(pool.TryGet(out var c));

        var before = DateTimeOffset.UtcNow;
        c = await pool.TryGetOrWaitAsync();
        var after = DateTimeOffset.UtcNow;

        Assert.Equal(3, c.Instance.Value);

        var waitingTime = after - before;
        Assert.True(waitingTime.TotalMilliseconds > 950 && waitingTime.TotalMilliseconds < 1050);

        Assert.True(pool.TryGet(out var d));
        Assert.Equal(4, d.Instance.Value);

        Assert.False(pool.TryGet(out var e));
    }


    [Fact]
    public async Task Pool_AsyncRelease()
    {
        int i = 0;
        var pool = new Pool<PoolItem>(2, TimeSpan.Zero, () => new(++i));
        Assert.True(pool.TryGet(out var a));
        Assert.Equal(1, a.Instance.Value);
        Assert.True(pool.TryGet(out var b));
        Assert.Equal(2, b.Instance.Value);
        Assert.False(pool.TryGet(out var c));

        var before = DateTimeOffset.UtcNow;
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);
            a.Release();
        });
        c = await pool.TryGetOrWaitAsync();
        var after = DateTimeOffset.UtcNow;
        Assert.Equal(a.Instance.Value, c.Instance.Value);

        Assert.False(pool.TryGet(out var d));

        var waitingTime = after - before;
        Assert.True(waitingTime.TotalMilliseconds > 950 && waitingTime.TotalMilliseconds < 1050);
    }

    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }
    }

    [Fact]
    public void InfileConnection()
    {
        var path = @"c:\temp";

        var connection = new JsonInfileConnection<TestModel>(path);
        connection.Collection.Clear();

        var massiveCount = 40000;
        var chunkSize = 10000;
        var massiveItems = new TestModel[chunkSize];
        for (var c = 0; c < massiveCount; c += chunkSize)
        {
            for (var i = 0; i < chunkSize; i++)
            {
                massiveItems[i] = new TestModel
                {
                    Age = c + i,
                    Name = "Roei" + (c + i),
                    UserName = Infrastructure.Utils.Security.UserSecurity.Default.GeneratePassword(),
                    Email = Infrastructure.Utils.Security.UserSecurity.Default.GeneratePassword() + "@gmail.com",
                    Password = Infrastructure.Utils.Security.UserSecurity.Default.GeneratePassword()
                };
            }
            connection.Collection.AddRange(massiveItems);
        }
        connection.Dispose();

        connection = new JsonInfileConnection<TestModel>(path);
        Assert.Equal(connection.Collection.Count, massiveCount);
        connection.Collection.FirstOrDefault(x => x.Age == massiveCount - 1);
        connection.Collection.Clear();

        connection.Collection.Add(new TestModel
        {
            Age = 4,
            Name = "myname"
        });

        var connection2 = new JsonInfileConnection<TestModel>(path);
        Assert.Single(connection.Collection);
        connection2.Dispose();

        connection.Dispose();

        connection = new JsonInfileConnection<TestModel>(path);
        Assert.True(connection.Collection.Count == 1);
        Assert.True(connection.Collection.First().Age == 4);
        Assert.True(connection.Collection.First().Name == "myname");
        connection.Collection.Add(new TestModel
        {
            Age = 8,
            Name = "myname2"
        });
        connection.Dispose();


        connection = new JsonInfileConnection<TestModel>(path);
        Assert.True(connection.Collection.Count == 2);
        Assert.True(connection.Collection.First().Age == 4);
        Assert.True(connection.Collection.Last().Age == 8);
        Assert.True(connection.Collection.First().Name == "myname");
        Assert.True(connection.Collection.Last().Name == "myname2");
        var first = connection.Collection.First();
        first.Name = "first";
        connection.Collection.Replace(first);
        connection.Dispose();


        connection = new JsonInfileConnection<TestModel>(path);
        Assert.True(connection.Collection.Count == 2);
        Assert.True(connection.Collection.Last().Age == 4);
        Assert.True(connection.Collection.First().Age == 8);
        Assert.True(connection.Collection.Last().Name == "first");
        Assert.True(connection.Collection.First().Name == "myname2");
        first = connection.Collection.Last();
        connection.Collection.Remove(first);
        connection.Dispose();

        connection = new JsonInfileConnection<TestModel>(path);
        Assert.True(connection.Collection.Count == 1);
        Assert.True(connection.Collection.First().Age == 8);
        Assert.True(connection.Collection.First().Name == "myname2");
        first = connection.Collection.First();
        connection.Collection.Remove(first);
        connection.Dispose();

        connection = new JsonInfileConnection<TestModel>(path);
        Assert.True(connection.Collection.Count == 0);
        connection.Collection.Clear();
        connection.Dispose();

        connection = new JsonInfileConnection<TestModel>(path);
        var roei1 = new TestModel
        {
            Age = 1,
            Name = "myname1"
        };
        connection.Collection.Add(roei1);
        var roei2 = new TestModel
        {
            Age = 2,
            Name = "myname2"
        };
        connection.Collection.Add(roei2);
        roei1.Name = "myname-1";
        connection.Collection.Replace(roei1);
        connection.Collection.Replace(roei1);
        connection.Collection.Replace(roei1);
        connection.Collection.Replace(roei1);
        connection.Collection.Remove(roei1);
        connection.Collection.Optimize();
        connection.Dispose();

        connection = new JsonInfileConnection<TestModel>(path);
        Assert.True(connection.Collection.Count == 1);
        Assert.True(connection.Collection.First().Age == 2);
        Assert.True(connection.Collection.First().Name == "myname2");
        connection.Collection.Clear();
        connection.Dispose();

    }


    [Fact]
    public void PersistentCollection()
    {
        var path = @"c:\temp";

        var noIndexStorage = new JsonPersistentCollection<TestModel>(path, useIndexFile: false)
        {
            new() { Name = "no-index" },
            new() { Name = "no-index-2" },
            new() { Name = "no-index-3" }
        };
        noIndexStorage.RemoveAt(1);
        noIndexStorage.Dispose();

        noIndexStorage = new JsonPersistentCollection<TestModel>(path, useIndexFile: false);
        Assert.Equal(2, noIndexStorage.Count);
        Assert.Equal("no-index", noIndexStorage[0].Name);
        Assert.Equal("no-index-3", noIndexStorage[1].Name);
        noIndexStorage.Clear();

    }
}
