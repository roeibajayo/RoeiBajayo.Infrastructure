using System;
using System.Collections.Generic;
using System.Globalization;

namespace Infrastructure.Utils.Dates;

public readonly struct JewishDate(int hebDay, int hebMonth, int hebYear)
{
    public int Day { get; init; } = hebDay;
    public int Month { get; init; } = hebMonth;
    public int Year { get; init; } = hebYear;

    public enum Cities { Raanana, TelAviv, KfarYona };
    private readonly static HebrewCalendar _hebrewCalendarHelper = new();
    private readonly static Dictionary<Cities, Tuple<double, double>> _citiesLatitude =
        new()
        {
            { Cities.Raanana, new Tuple<double, double>(32.183364, 34.870671) },
            { Cities.TelAviv, new Tuple<double, double>(32.08088, 34.78057) },
            { Cities.KfarYona, new Tuple<double, double>(32.31387, 34.93202) },
            //{ Cities.Jerusalem, new Tuple<double, double>(31.771575, 35.217072) }
        };

    public static JewishDate FromDateTime(DateTime date)
    {
        return new JewishDate(_hebrewCalendarHelper.GetDayOfMonth(date),
            _hebrewCalendarHelper.GetMonth(date),
            _hebrewCalendarHelper.GetYear(date));
    }

    public static JewishDate Now =>
        FromDateTime(DateTime.Now);
    public static JewishDate UtcNow =>
        FromDateTime(DateTime.UtcNow);

    public string? GetHoliday(bool outOfIsrael = false)
    {
        var hebMonth = Month;

        var leapMonth = _hebrewCalendarHelper.GetLeapMonth(Year,
            _hebrewCalendarHelper.GetEra(ToDateTime()));

        if (leapMonth != 0 && hebMonth >= leapMonth)
            hebMonth--;

        if (hebMonth == 1)
        {
            if (Day == 1)
                return "ראש השנה";

            if (Day == 2)
                return "ראש השנה";

            if (GetWeekday(3, 7, Year) == 6)
            {
                if (Day == 4)
                    return "צום גדליה";
            }
            else
            {
                if (Day == 3)
                    return "צום גדליה";
            }

            if (Day == 9)
                return "ערב יום כיפור";

            if (Day == 10)
                return "יום כיפור";

            if (Day == 14)
                return "ערב סוכות";

            if (Day == 15)
                return "סוכות";

            if (Day == 16)
                return outOfIsrael ? "סוכות, יום טוב שני של גלויות" : "חול המועד";

            if (Day == 17 || Day == 18 || Day == 19 || Day == 20 || Day == 21)
                return "הושענא רבה";

            if (Day == 22)
                return outOfIsrael ? "שמיני עצרת" : "שמיני עצרת, שמחת תורה";

            if (Day == 23 && outOfIsrael)
                return "שמחת תורה";
        }
        if (hebMonth == 3)
        {
            if (Day == 25)
                return "חנוכה, יום א";

            if (Day == 26)
                return "חנוכה, יום ב";

            if (Day == 27)
                return "חנוכה, יום ג";

            if (Day == 28)
                return "חנוכה, יום ד";

            if (Day == 29)
                return "חנוכה, יום ה";

            if (Day == 30 && _hebrewCalendarHelper.GetDaysInMonth(Year, 9) == 30)
                return "חנוכה, יום ו";
        }
        if (hebMonth == 4)
        {
            if (Day == 10)
                return "צום עשרה בטבת";

            if (_hebrewCalendarHelper.GetDaysInMonth(Year, 9) == 30)
            {
                if (Day == 1)
                    return "חנוכה, יום ז";
                if (Day == 2)
                    return "חנוכה, יום ח";
            }
            if (_hebrewCalendarHelper.GetDaysInMonth(Year, 9) == 29)
            {
                if (Day == 1)
                    return "חנוכה, יום ו";
                if (Day == 2)
                    return "חנוכה, יום ז";
                if (Day == 3)
                    return "חנוכה, יום ח";
            }
        }
        if (hebMonth == 5)
        {
            if (Day == 15)
                return "טו בשבט";
        }
        if (hebMonth == 6)
        {
            if (IsLeapMonth)
            {
                if (GetWeekday(13, 6, Year) == 6)
                {
                    if (Day == 11)
                        return "תענית אסתר";
                }
                else
                {
                    if (Day == 13)
                        return "תענית אסתר";
                }

                if (Day == 14)
                    return "פורים";

                if (Day == 15)
                    return "שושן פורים";
            }
            else
            {
                if (Day == 14)
                    return "פורים קטן";

                if (Day == 15)
                    return "שושן פורים קטן";
            }
        }
        if (hebMonth == 7)
        {
            int hagadolDay = 14;
            while (GetWeekday(hagadolDay, 1, Year) != 6)
                hagadolDay -= 1;

            if (Day == hagadolDay)
                return "שבת הגדול";

            if (Day == 14)
                return "ערב פסח";

            if (Day == 15)
                return "פסח";

            if (Day == 16)
            {
                if (outOfIsrael)
                {
                    return "פסח, יום טוב שני של גלויות";
                }
                else
                {
                    return "חול המועד פסח";
                }
            }
            if (Day == 17 || Day == 18 || Day == 19)
                return "חול המועד פסח";

            if (Day == 20)
                return "ערב שביעי של פסח";

            if (Day == 21)
                return "שביעי של פסח";

            if (Day == 22)
                return outOfIsrael ? "שביעי של פסח, יום טוב שני של גלויות" : "אסרו חג";

            if (GetWeekday(27, 1, Year) == 5)
            {
                if (Day == 26)
                    return "יום השואה";
            }
            else if (Year >= 5757 && GetWeekday(27, 1, Year) == 0)
            {
                if (Day == 28)
                    return "יום השואה";
            }
            else
            {
                if (Day == 27)
                    return "יום השואה";
            }

        }
        if (hebMonth == 8)
        {
            if (GetWeekday(4, 2, Year) == 5)
            {
                if (Day == 2)
                    return "יום הזכרון לשואה ולגבורה";
            }
            else if (GetWeekday(4, 2, Year) == 4)
            {
                if (Day == 3)
                    return "יום הזכרון לשואה ולגבורה";
            }
            else if (Year >= 5764 && GetWeekday(4, 2, Year) == 0)
            {
                if (Day == 5)
                    return "יום הזכרון לשואה ולגבורה";
            }
            else
            {
                if (Day == 4)
                    return "יום הזכרון לשואה ולגבורה";
            }

            if (GetWeekday(5, 2, Year) == 6)
            {
                if (Day == 3)
                    return "יום העצמאות";
            }
            else if (GetWeekday(5, 2, Year) == 5)
            {
                if (Day == 4)
                    return "יום העצמאות";
            }
            else if (Year >= 5764 && GetWeekday(4, 2, Year) == 0)
            {
                if (Day == 6)
                    return "יום העצמאות";
            }
            else
            {
                if (Day == 5)
                    return "יום העצמאות";
            }

            if (Day == 14)
                return "חג פסח שני";

            if (Day == 18)
                return "לג בעומר";

            if (Day == 28)
                return "יום ירושלים";
        }
        if (hebMonth == 9)
        {
            if (Day == 5)
                return "ערב שבועות";

            if (Day == 6)
                return "שבועות";

            if (Day == 7 && outOfIsrael)
                return "שבועות, יום טוב שני של גלויות";
        }
        if (hebMonth == 10)
        {
            if (GetWeekday(17, 4, Year) == 6)
            {
                if (Day == 18)
                    return "צום יז בתמוז";
            }
            else
            {
                if (Day == 17)
                    return "צום יז בתמוז";
            }
        }
        if (hebMonth == 11)
        {
            if (GetWeekday(9, 5, Year) == 6)
            {
                if (Day == 10)
                    return "צום תשעה באב";
            }
            else
            {
                if (Day == 9)
                    return "צום תשעה באב";
            }

            if (Day == 15)
                return "טו באב";
        }
        if (hebMonth == 12)
        {
            if (Day == 29)
                return "ערב ראש השנה";
        }
        return null;
    }

    private static int GetWeekday(int hebDay, int hebMonth, int hebYear)
    {
        var date = _hebrewCalendarHelper.ToDateTime(hebYear, hebMonth, hebDay, 0, 0, 0, 0);
        return (int)_hebrewCalendarHelper.GetDayOfWeek(date);
    }

    private Dictionary<string, TimeSpan> GetDayTimes(double lat, double lng, TimeZoneInfo? timezone)
    {
        var date = ToDateTime();
        var offset = (timezone ?? TimeZoneInfo.Local).GetUtcOffset(DateTime.UtcNow);

        var result = SunriseSunset.GetUTC(date.Year, date.Month, date.Day, lat, lng);

        var zrihaTime = offset + result.Sunrise;
        var shkiaTime = offset + result.Sunset;

        var zmanit = (shkiaTime - zrihaTime) / 12;
        var shaaZmanit = zmanit.TotalHours;
        var dakaZmanit = zmanit.TotalMinutes / 60;

        var hatzutOffset = TimeSpan.FromHours(shaaZmanit * 6);
        var hatzutLaila = zrihaTime + hatzutOffset + TimeSpan.FromHours(12);
        if (hatzutLaila.Days > 0)
        {
            hatzutLaila = hatzutLaila.Add(TimeSpan.FromDays(-1));
        }

        var alotHaShaharTime = zrihaTime -
            TimeSpan.FromMinutes(72 * dakaZmanit);

        var tzetHaCochavimTime = shkiaTime +
            TimeSpan.FromMinutes(13.5 * dakaZmanit);

        var rabenuTamTime = shkiaTime +
            TimeSpan.FromMinutes(72 * dakaZmanit);

        var magenAvrahamHour = (rabenuTamTime - alotHaShaharTime) / 12;
        var gaonMeVilnaHour = zmanit;

        return new Dictionary<string, TimeSpan>
        {
            { "עלות השחר", alotHaShaharTime },
            { "טלית ותפילין", alotHaShaharTime + TimeSpan.FromMinutes(dakaZmanit * 6) },
            { "זריחה", zrihaTime + TimeSpan.FromMinutes(dakaZmanit * 6) },
            { "סוף זמן ק\"ש מג\"א", alotHaShaharTime + (magenAvrahamHour * 3) },
            { "סוף זמן ק\"ש גר\"א", zrihaTime + (gaonMeVilnaHour * 3) },
            //{ "סוף זמן תפילה מג\"א", alotHaShaharTime + (magenAvrahamHour * 4) },
            { "סוף זמן תפילה", zrihaTime + (gaonMeVilnaHour * 4) },
            { "חצות היום", zrihaTime + hatzutOffset },
            { "מנחה גדולה", zrihaTime + hatzutOffset + TimeSpan.FromMinutes(30) },
            { "מנחה קטנה", zrihaTime + TimeSpan.FromHours(shaaZmanit * 9.5) },
            { "פלג המנחה", shkiaTime - TimeSpan.FromHours(shaaZmanit * 1.025) },
            { "שקיעה", shkiaTime },
            { "צאת הכוכבים", tzetHaCochavimTime },
            { "צאת הכוכבים רבנו תם", rabenuTamTime },
            { "חצות הלילה", hatzutLaila }
        };
    }

    public Dictionary<string, TimeSpan> GetDayTimes(Cities city, TimeZoneInfo? timezone = null)
    {
        var lat = _citiesLatitude[city];
        return GetDayTimes(lat.Item1, lat.Item2, timezone);
    }

    public DateTime ToDateTime() =>
        _hebrewCalendarHelper.ToDateTime(Year, Month, Day, 0, 0, 0, 0);

    public bool IsLeapMonth =>
        _hebrewCalendarHelper.IsLeapMonth(Year, Month);
    public bool IsLeapYear =>
        _hebrewCalendarHelper.IsLeapYear(Year);

    public override int GetHashCode()
    {
        return (Year - 1583) * 366 + Month * 31 + Day;
    }
    public override string ToString()
    {
        return ToDateTime().ToJewishDateString();
    }
    public string ToString(string format)
    {
        return ToDateTime().ToJewishDateString(format);
    }
}