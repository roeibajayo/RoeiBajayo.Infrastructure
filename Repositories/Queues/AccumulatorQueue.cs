using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;

namespace Infrastructure.Utils.Repositories.Queues;

/// <summary>
/// Accumulate items and bulk process them. 
/// example: accumulate logs and write all once.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class AccumulatorQueue<T> : IDisposable
{
    private List<T> _bank;
    private readonly System.Timers.Timer _timer;
#if NET9_0_OR_GREATER
    private readonly Lock EXECUTE_LOCER = new();
#else
    private readonly object EXECUTE_LOCER = new();
#endif

    private Action<IEnumerable<T>>? _onProcessAction;
    private Func<IEnumerable<T>, Task>? _onProcessAsyncAction;

    public event ProcessFailedHandler<T>? ProcessFailed;
    private bool _enabled = true;

    public int MaxItemsPerBulk { get; set; }

    /// <param name="maxItemsPerBulk">0 = unlimited</param>
    /// <param name="interval">in milliseconds</param>
    public AccumulatorQueue(Func<IEnumerable<T>, Task> processAction,
        int interval,
        int maxItemsPerBulk = 0) : this(interval, maxItemsPerBulk)
    {
        ArgumentNullException.ThrowIfNull(processAction);

        SetProcessAction(processAction);
    }

    /// <param name="maxItemsPerBulk">0 = unlimited</param>
    /// <param name="interval">in milliseconds</param>
    public AccumulatorQueue(Action<IEnumerable<T>> processAction,
        int interval,
        int maxItemsPerBulk = 0) : this(interval, maxItemsPerBulk)
    {
        ArgumentNullException.ThrowIfNull(processAction);

        SetProcessAction(processAction);
    }

    /// <param name="maxItemsPerBulk">0 = unlimited</param>
    /// <param name="interval">in milliseconds</param>
    public AccumulatorQueue(int interval,
        int maxItemsPerBulk = 0)
    {
        if (interval <= 0)
            throw new ArgumentException(null, nameof(interval));

        MaxItemsPerBulk = maxItemsPerBulk < 0 ? 0 : maxItemsPerBulk;

        _bank = [];
        _timer = new System.Timers.Timer(interval);
        _timer.Elapsed += IntervalTimer_Elapsed!;
    }

    public int Interval
    {
        get => (int)_timer.Interval;
        set => _timer.Interval = value;
    }

    public void SetProcessAction(Action<IEnumerable<T>> processAction)
    {
        _onProcessAsyncAction = null;
        _onProcessAction = processAction ?? throw new ArgumentNullException(nameof(processAction));
    }
    public void SetProcessAction(Func<IEnumerable<T>, Task> processAction)
    {
        _onProcessAction = null;
        _onProcessAsyncAction = processAction ?? throw new ArgumentNullException(nameof(processAction));
    }

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (value != _enabled)
            {
                _enabled = value;

                if (_enabled)
                    _timer.Start();
                else
                    _timer.Stop();
            }
        }
    }

    public void ForceExecute() =>
        IntervalTimer_Elapsed(this, null);

    void IntervalTimer_Elapsed(object sender, ElapsedEventArgs? e)
    {
        lock (EXECUTE_LOCER)
        {
            if (_bank.Count != 0)
            {
                if (MaxItemsPerBulk == 0 || _bank.Count <= MaxItemsPerBulk)
                {
                    var oldBank = _bank;
                    _bank = new List<T>(MaxItemsPerBulk);
                    Execute(oldBank);
                    oldBank.Clear();
                }
                else
                {
                    var nextBulk = _bank.GetRange(0, MaxItemsPerBulk);
                    _bank.RemoveRange(0, MaxItemsPerBulk);
                    Execute(nextBulk);
                }
            }
            else
            {
                _timer.Stop();
            }
        }
    }

    private void Execute(IEnumerable<T> items)
    {
        try
        {
            if (_onProcessAsyncAction == null)
                _onProcessAction!(items);
            else
                _onProcessAsyncAction(items).GetAwaiter().GetResult();

        }
        catch (Exception e)
        {
            if (ProcessFailed == null)
                throw;

            ProcessFailed(this, e);
        }
    }

    public void Enqueue(T item)
    {
        _bank.Add(item);

        if (_enabled)
            _timer.Start();
    }
    public void Enqueue(IEnumerable<T> items)
    {
        _bank.AddRange(items);

        if (_enabled)
            _timer.Start();
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}

public delegate void ProcessFailedHandler<T>(AccumulatorQueue<T> sender, Exception exception);
