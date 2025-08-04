using RoeiBajayo.Infrastructure.Threads;
using System.Reflection;
using System.Threading.Tasks;

namespace System.Timers;

public static class TimerExtensions
{
    public static Task StartAt(this Timer timer, DateTime date,
        bool executeOnStart = true)
    {
        return Tasks.StartAt(() =>
        {
            if (executeOnStart)
                timer.Execute();

            timer.Start();
        }, date);
    }
    public static Task StopAt(this Timer timer, DateTime date)
    {
        return Tasks.StartAt(timer.Stop, date);
    }
    public static Task ExecuteAt(this Timer timer, DateTime date)
    {
        return Tasks.StartAt(timer.Execute, date);
    }

    public static void Execute(this Timer timer)
    {
        var invokerField = typeof(Timer).GetField("_onIntervalElapsed",
                BindingFlags.NonPublic | BindingFlags.Instance);

        (invokerField!.GetValue(timer) as ElapsedEventHandler)!(timer, null!);
    }
}
