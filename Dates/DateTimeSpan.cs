using System;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Utils.Dates;

public sealed class DateTimeSpan
{
    public SortedSet<int>? DaysOfMonth { get; init; }
    public SortedSet<DayOfWeek>? DaysOfWeek { get; init; }
    public required TimeOnly Time { get; init; }
    public TimeSpan DurationTime { get; init; } = TimeSpan.Zero;
    public TimeOnly ToTime() => Time.Add(DurationTime);

    public DateTimeOffset GetNextStart(DateTimeOffset relativeTo) =>
        GetNextStart(relativeTo, 1);
    public DateTimeOffset GetPrevStart(DateTimeOffset relativeTo) =>
        GetNextStart(relativeTo, -1);
    private DateTimeOffset GetNextStart(DateTimeOffset relativeTo, int negative)
    {
        var currentStart = new DateTimeOffset(new DateTime(relativeTo.Year, relativeTo.Month, relativeTo.Day,
            Time.Hour, Time.Minute, Time.Second));
        var justReset = false;
        while (negative == -1 ? currentStart > relativeTo : currentStart <= relativeTo)
        {
            if (justReset)
            {
                justReset = false;
            }
            else
            {
                currentStart = currentStart.AddDays(1 * negative);
            }

            if (DaysOfWeek is not null and { Count: not 0 })
            {
                var nextDay = false;
                if (!DaysOfWeek.Contains(currentStart.DayOfWeek))
                {
                    var thisWeek = DaysOfWeek.SkipWhile(x => x < currentStart.DayOfWeek).FirstOrDefault();
                    if (thisWeek > 0)
                    {
                        currentStart = currentStart.AddDays((thisWeek - currentStart.DayOfWeek) * negative);
                    }
                    else
                    {
                        currentStart = currentStart.AddDays((DaysOfWeek.First() + 7 - currentStart.DayOfWeek) * negative);
                    }
                    nextDay = true;
                }

                if (nextDay)
                {
                    currentStart = ResetTime(currentStart);
                    justReset = true;
                    continue;
                }
            }

            if (DaysOfMonth is not null and { Count: not 0 })
            {
                var nextDay = false;
                if (!DaysOfMonth.Contains(currentStart.Day))
                {
                    var thisMonth = DaysOfMonth.SkipWhile(x => x < currentStart.Day).FirstOrDefault();
                    if (thisMonth > 0)
                    {
                        currentStart = currentStart.AddDays((thisMonth - currentStart.Day) * negative);
                    }
                    else
                    {
                        currentStart = currentStart.AddDays((currentStart.Day - DaysOfMonth.First()) * -1 * negative);
                        currentStart = currentStart.AddMonths(1 * negative);
                    }
                    nextDay = true;
                }

                if (nextDay)
                {
                    currentStart = ResetTime(currentStart);
                    justReset = true;
                    continue;
                }
            }
        }

        return currentStart;
    }
    private DateTimeOffset ResetTime(DateTimeOffset date)
    {
        return date.Date
             .AddHours(Time.Hour)
             .AddMinutes(Time.Minute)
             .AddSeconds(Time.Second);
    }

    public bool IsInRange() => IsInRange(DateTimeOffset.Now);
    public bool IsInRange(DateTimeOffset date)
    {
        var time = new TimeOnly(date.TimeOfDay.Ticks);
        return (DaysOfMonth == null || DaysOfMonth.Count == 0 || DaysOfMonth.Contains(date.Day)) &&
            (DaysOfWeek == null || DaysOfWeek.Count == 0 || DaysOfWeek.Contains(date.DayOfWeek)) &&
            Time <= time && ToTime() >= time;
    }
    public bool IsInRange(DateOnly date)
    {
        return (DaysOfMonth == null || DaysOfMonth.Count == 0 || DaysOfMonth.Contains(date.Day)) &&
            (DaysOfWeek == null || DaysOfWeek.Count == 0 || DaysOfWeek.Contains(date.DayOfWeek));
    }
}
