using System.Globalization;

namespace System;

/// <summary>
/// Provides extension methods for the <see cref="DateTime"/> class.
/// </summary>
public static class DateTimeExtensions
{
    public static bool IsBetween(this DateTime date, DateTimeOffset from, DateTimeOffset to) =>
        from <= date && date <= to;
    public static bool IsBetween(this DateTimeOffset date, DateTimeOffset from, DateTimeOffset to) =>
        from <= date && date <= to;

    public static long ToUnixTime(this DateTime date) =>
        Convert.ToInt64((date.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds);
    public static DateTime FromUnixTime(this long unixTimestamp)
    {
        // Check if the timestamp is in milliseconds and convert to seconds if necessary
        if (unixTimestamp > 9999999999)
            unixTimestamp /= 1000;

        return DateTime.UnixEpoch + TimeSpan.FromSeconds(unixTimestamp);
    }

    public static string ToJewishDateString(this DateTime value, string format = "d MMM, y")
    {
        var ci = CultureInfo.CreateSpecificCulture("he-IL");
        ci.DateTimeFormat.Calendar = new HebrewCalendar();
        return value.ToString(format, ci).Replace("'", "");
    }

    public static string ToPublishDuration(this DateTime date)
    {
        var result = (DateTime.UtcNow - date.ToUniversalTime()).TotalSeconds;
        if (result < 60)
        {
            return "ממש הרגע";
        }

        result /= 60;

        string text;
        if (result < 60)
        {
            result = Math.Round(result);
            if (result == 1)
            {
                text = "דקה";
            }
            else
            {
                text = (int)result + " דקות";
            }
        }
        else
        {
            result /= 60;
            if (result < 24)
            {
                result = Math.Round(result);
                if (result == 1)
                {
                    text = "שעה";
                }
                else if (result == 2)
                {
                    text = "שעתיים";
                }
                else
                {
                    text = (int)result + " שעות";
                }
            }
            else
            {
                result /= 24;
                if (result < 30)
                {
                    result = Math.Round(result);
                    if (result == 1)
                    {
                        return "אתמול";
                    }
                    else if (result == 2)
                    {
                        text = "יומיים";
                    }
                    else
                    {
                        text = (int)result + " ימים";
                    }
                }
                else
                {
                    result /= 30;
                    if (result < 12)
                    {
                        result = Math.Floor(result);

                        if (result == 1)
                        {
                            text = "חודש";
                        }
                        else if (result == 2)
                        {
                            text = "חודשיים";
                        }
                        else
                        {
                            text = (int)result + " חודשים";
                        }
                    }
                    else
                    {
                        return "מעל שנה";
                    }
                }
            }
        }

        return "לפני " + text;
    }
    public static string ToHumanString(this TimeSpan duration)
    {
        if (duration.TotalSeconds < 60)
            return (int)duration.TotalSeconds + " שניות";

        var minutes = (int)duration.TotalMinutes;
        if (minutes > 59)
        {
            var hours = (int)duration.TotalHours;
            minutes -= hours * 60;
            return (hours > 9 ? hours.ToString() : "0" + hours) + ":" + (minutes > 9 ? minutes.ToString() : "0" + minutes) + " שעות";
        }
        else
        {
            return (minutes > 9 ? minutes.ToString() : "0" + minutes) + " דקות";
        }

    }
    public static int GetAge(this DateTime birthDate)
    {
        //return (int)((DateTime.UtcNow - birthDate).TotalDays / 365.25D);

        var now = DateTime.UtcNow;
        var age = now.Year - birthDate.Year;

        if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day))
            age--;

        return age;
    }


    public static DateTime GetStartDayOfWeek(this DateTime date, DayOfWeek startDay = DayOfWeek.Sunday)
    {
        var currentDay = date.DayOfWeek;

        int diff = (7 + (currentDay - startDay)) % 7;
        return date.AddDays(-1 * diff).Date;
    }
    public static DateTime GetEndDayOfWeek(this DateTime date, DayOfWeek startDay = DayOfWeek.Sunday)
    {
        return date.GetStartDayOfWeek(startDay).AddDays(7).AddTicks(-1);
    }

    public static DateTime GetStartOfMonth(DateTime date)
    {
        var startOfMonth = new DateTime(date.Year, date.Month, 1);
        return startOfMonth;
    }
    public static DateTime GetEndOfMonth(DateTime date)
    {
        var startOfMonth = GetStartOfMonth(date);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);
        return endOfMonth;
    }
}