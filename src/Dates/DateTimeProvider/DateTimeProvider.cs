using System;

namespace RoeiBajayo.Infrastructure.Dates.DateTimeProvider;

internal class DateTimeProvider : IDateTimeProvider
{
    private TimeZoneInfo? timeZoneInfo = null;
    private readonly Lazy<TimeZoneInfo> israelTimeZoneInfo =
        new(() => TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time"));
    private TimeSpan? DiffFromNow = null;

    public void SetTimeZone(string? timeZoneId)
    {
        if (timeZoneId is null)
            return;

        timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    public DateTime Now =>
        ToTimeZone(DateTime.Now);

    public DateTime UtcNow =>
        DateTime.UtcNow;

    public DateTime Today =>
        ToTimeZone(DateTime.Now).Date;

    public DateTime IsraelNow
    {
        get
        {
            var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, israelTimeZoneInfo.Value);

            if (DiffFromNow.HasValue)
                now += DiffFromNow.Value;

            return now;
        }
    }

    public DateTime Create(int year, int month, int day, int hour, int minute, int second) =>
        new(year, month, day, hour, minute, second, DateTimeKind.Unspecified);

    public DateTime ToTimeZone(DateTime utcDateTime, bool ignoreNowChange = false)
    {
        var now = timeZoneInfo is null ? utcDateTime : TimeZoneInfo.ConvertTime(utcDateTime, timeZoneInfo);
        if (!ignoreNowChange && DiffFromNow.HasValue)
            now += DiffFromNow.Value;
        return now;
    }
    public DateTime ToUtc(DateTime tzDateTime, bool ignoreNowChange = false)
    {
        var now = tzDateTime - timeZoneInfo!.BaseUtcOffset;
        if (!ignoreNowChange && DiffFromNow.HasValue)
            now -= DiffFromNow.Value;
        return new DateTime(now.Ticks, DateTimeKind.Utc);
    }

    public void SetNowDate(DateTime now)
    {
        var realNow = Now;
        DiffFromNow = now - realNow;
    }
}
