using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace RoeiBajayo.Infrastructure.Dates.DateTimeProvider;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    DateTime IsraelNow { get; }
    DateTime Today { get; }

    DateTime Create(int year, int month, int day, int hour = 0, int minute = 0, int second = 0);
    DateTime ToTimeZone(DateTime utcDateTime, bool ignoreNowChange = false);
    DateTime? ToTimeZone(DateTime? utcDateTime, bool ignoreNowChange = false) =>
        utcDateTime.HasValue ? ToTimeZone(utcDateTime.Value, ignoreNowChange) : null;
    DateTime ToUtc(DateTime tzDateTime, bool ignoreNowChange = false);
    DateTime? ToUtc(DateTime? tzDateTime, bool ignoreNowChange = false) =>
        tzDateTime.HasValue ? ToUtc(tzDateTime.Value, ignoreNowChange) : null;
    void SetTimeZone(string? timeZoneId);
    void SetNowDate(DateTime now);
}

public static class DateTimeProviderExtensions
{
    public static void AddDateTimeProvider(this IServiceCollection services, string? timeZoneId = null)
    {
        var dateTimeProvider = new DateTimeProvider();
        dateTimeProvider.SetTimeZone(timeZoneId);
        services.RemoveAll<IDateTimeProvider>();
        services.AddSingleton<IDateTimeProvider>(dateTimeProvider);
    }
}