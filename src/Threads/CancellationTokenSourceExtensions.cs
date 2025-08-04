using System;
using System.Threading;

namespace RoeiBajayo.Infrastructure.Threads;

public static class CancellationTokenSourceExtensions
{
    public static void CancelOn(this CancellationTokenSource cts, DateTime cancelTime) =>
        cts.CancelOn(DateTime.Now, cancelTime);
    public static void CancelOn(this CancellationTokenSource cts, DateTime now, DateTime cancelTime)
    {
        if (cancelTime <= now)
        {
            cts.Cancel();
            return;
        }

        cts.CancelAfter((int)(cancelTime - now).TotalMilliseconds);
    }
}
