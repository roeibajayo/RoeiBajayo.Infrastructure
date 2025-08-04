using System;
using System.Collections.Generic;
using System.Linq;

namespace RoeiBajayo.Infrastructure.Dates;

public sealed class DateTimeRepeats
{
    public required DateOnly StartDate { get; init; }
    public required DateTimeSpan Span { get; init; }
    public DateTimeOffset? End { get; init; }
    public int? Count { get; init; }
    public TimeSpan? RepeatEvery { get; init; }

    private IReadOnlyList<DateRange>? dates;
    public IReadOnlyList<DateRange> Dates
    {
        get
        {
            dates ??= GetAllDates().ToArray();
            return dates;
        }
    }

    public IEnumerable<DateRange> GetRepeats()
    {
        var currentStart = new DateTimeOffset(new DateTime(StartDate.Year, StartDate.Month, StartDate.Day,
            Span.Time.Hour, Span.Time.Minute, Span.Time.Second));
        currentStart += Span.DurationTime;
        return GetAllNext(currentStart);
    }

    public DateRange? GetNext(DateTimeOffset relativeTo)
    {
        var next = GetAllNext(relativeTo)
            .FirstOrDefault();
        return next.Start == DateTimeOffset.MinValue ? null : next;
    }
    public IEnumerable<DateRange> GetAllNext(DateTimeOffset relativeTo)
    {
        return Dates
            .SkipWhile(x => x.Start < relativeTo || x.End <= relativeTo);
    }

    public DateRange? GetPrev(DateTimeOffset relativeTo)
    {
        var prev = GetAllPrev(relativeTo)
            .LastOrDefault();
        return prev.Start == DateTimeOffset.MinValue ? null : prev;
    }
    public IEnumerable<DateRange> GetAllPrev(DateTimeOffset relativeTo)
    {
        return Dates
            .TakeWhile(x => x.End < relativeTo);
    }

    public bool IsInRange(DateTimeOffset date)
    {
        return Dates
            .Any(x => date.IsBetween(x.Start, x.End));
    }
    public bool IsInRange(DateTimeOffset start, DateTimeOffset end)
    {
        return Dates
            .Any(x => start.IsBetween(x.Start, x.End) || end.IsBetween(x.Start, x.End));
    }

    private IEnumerable<DateRange> GetAllDates()
    {
        if (Span == null)
            throw new ArgumentNullException();

        if (End == null && Count == null)
            throw new StackOverflowException();

        var currentStart = new DateTimeOffset(new DateTime(StartDate.Year, StartDate.Month, StartDate.Day,
            Span.Time.Hour, Span.Time.Minute, Span.Time.Second));

        var currentCount = 0;
        while ((Count == null || currentCount <= Count) && (End == null || currentStart + Span.DurationTime < End))
        {
            yield return new() { Start = currentStart, End = currentStart + Span.DurationTime };
            currentCount++;

            if (RepeatEvery != null)
            {
                var nextRepeat = currentStart + RepeatEvery.Value;
                if (Span.IsInRange(new DateOnly(nextRepeat.Year, nextRepeat.Month, nextRepeat.Day)))
                {
                    currentStart = nextRepeat;
                    continue;
                }
            }

            currentStart = Span.GetNextStart(currentStart);
        }
    }

    public struct DateRange
    {
        public DateTimeOffset Start;
        public DateTimeOffset End;
    }
}
