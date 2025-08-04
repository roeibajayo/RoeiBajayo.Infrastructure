using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Utils.Threads;

public static partial class Tasks
{
    public static async Task<(T1, T2)> WhenAllAsync<T1, T2>(
        Task<T1> task1,
        Task<T2> task2)
    {
        await Task.WhenAll(new Task[] { task1, task2 }.Where(x => x is not null));
        return (task1.Result, task2.Result);
    }
    public static async Task<(T1, T2, T3)> WhenAllAsync<T1, T2, T3>(
        Task<T1> task1,
        Task<T2> task2,
        Task<T3> task3)
    {
        await Task.WhenAll(new Task[] { task1, task2, task3 }.Where(x => x is not null));
        return (task1.Result, task2.Result, task3.Result);
    }
    public static async Task<(T1, T2, T3, T4?)> WhenAllAsync<T1, T2, T3, T4>(
        Task<T1> task1,
        Task<T2> task2,
        Task<T3> task3,
        Task<T4> task4)
    {
        await Task.WhenAll(new Task[] { task1, task2, task3, task4 }.Where(x => x is not null));
        return (task1.Result, task2.Result, task3.Result, task4.Result);
    }
    public static async Task<(T1, T2, T3, T4, T5?)> WhenAllAsync<T1, T2, T3, T4, T5>(
        Task<T1> task1,
        Task<T2> task2,
        Task<T3> task3,
        Task<T4> task4,
        Task<T5> task5)
    {
        await Task.WhenAll(new Task[] { task1, task2, task3, task4, task5 }.Where(x => x is not null));
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result);
    }
    public static async Task<(T1, T2, T3, T4, T5, T6?)> WhenAllAsync<T1, T2, T3, T4, T5, T6>(
        Task<T1> task1,
        Task<T2> task2,
        Task<T3> task3,
        Task<T4> task4,
        Task<T5> task5,
        Task<T6> task6)
    {
        await Task.WhenAll(new Task[] { task1, task2, task3, task4, task5, task6 }.Where(x => x is not null));
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result);
    }
    public static async Task<(T1, T2, T3, T4, T5, T6, T7?)> WhenAllAsync<T1, T2, T3, T4, T5, T6, T7>(
        Task<T1> task1,
        Task<T2> task2,
        Task<T3> task3,
        Task<T4> task4,
        Task<T5> task5,
        Task<T6> task6,
        Task<T7> task7)
    {
        await Task.WhenAll(new Task[] { task1, task2, task3, task4, task5, task6, task7 }.Where(x => x is not null));
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result);
    }
    public static async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> WhenAllAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
        Task<T1> task1,
        Task<T2> task2,
        Task<T3> task3,
        Task<T4> task4,
        Task<T5> task5,
        Task<T6> task6,
        Task<T7> task7,
        Task<T8> task8)
    {
        await Task.WhenAll(new Task[] { task1, task2, task3, task4, task5, task6, task7, task8 }.Where(x => x is not null));
        return (task1.Result, task2.Result, task3.Result, task4.Result, task5.Result, task6.Result, task7.Result, task8.Result);
    }
}
