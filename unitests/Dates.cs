using RoeiBajayo.Infrastructure.Dates;
using Xunit;
using System;
using System.Linq;

namespace UnitTestProject;


public class Dates
{
    [Fact]
    public void JewishDate()
    {

        Assert.Equal("כ ניסן, תש\"פ",
            new DateTime(2020, 4, 14).ToJewishDateString());

        Assert.Equal("פורים קטן",
            RoeiBajayo.Infrastructure.Dates.JewishDate.FromDateTime(new DateTime(2019, 2, 19)).GetHoliday());

        Assert.Equal("תענית אסתר",
            RoeiBajayo.Infrastructure.Dates.JewishDate.FromDateTime(new DateTime(2019, 3, 20)).GetHoliday());

        Assert.Equal("ערב פסח",
            RoeiBajayo.Infrastructure.Dates.JewishDate.FromDateTime(new DateTime(2019, 4, 19)).GetHoliday());

        Assert.Equal("ערב שביעי של פסח",
            RoeiBajayo.Infrastructure.Dates.JewishDate.FromDateTime(new DateTime(2020, 4, 14)).GetHoliday());
    }

    [Fact]
    public void RepeatableDate()
    {
        var repeatable = new DateTimeRepeats
        {
            StartDate = new DateOnly(2023, 1, 1),
            Count = 3,
            RepeatEvery = TimeSpan.FromHours(1),
            Span = new DateTimeSpan
            {
                Time = new TimeOnly(12, 0),
                DaysOfWeek = [DayOfWeek.Sunday, DayOfWeek.Thursday],
                DurationTime = TimeSpan.FromHours(1)
            }
        };
        var dates = repeatable.GetRepeats().ToArray();
        Assert.Equal(3, dates.Length);

        Assert.Equal(null, repeatable.GetPrev(new DateTime(2023, 1, 1, 11, 59, 0)));
        Assert.Equal(null, repeatable.GetPrev(new DateTime(2023, 1, 1, 12, 0, 0)));
        Assert.Equal(null, repeatable.GetPrev(new DateTime(2023, 1, 1, 12, 1, 0)));
        Assert.Equal(null, repeatable.GetPrev(new DateTime(2023, 1, 1, 13, 0, 0)));
        Assert.NotEqual(null, repeatable.GetPrev(new DateTime(2023, 1, 1, 13, 1, 0)));
        Assert.Equal(null, repeatable.GetNext(new DateTime(2023, 1, 1, 16, 0, 0)));

        Assert.Equal(new DateTime(2023, 1, 1, 13, 0, 0), dates[0].Start);
        Assert.Equal(new DateTime(2023, 1, 1, 14, 0, 0), dates[0].End);
        Assert.Equal(new DateTime(2023, 1, 1, 14, 0, 0), dates[1].Start);
        Assert.Equal(new DateTime(2023, 1, 1, 15, 0, 0), dates[1].End);
        Assert.Equal(new DateTime(2023, 1, 1, 15, 0, 0), dates[2].Start);
        Assert.Equal(new DateTime(2023, 1, 1, 16, 0, 0), dates[2].End);

        repeatable = new DateTimeRepeats
        {
            StartDate = new DateOnly(2023, 1, 1),
            End = new DateTime(2023, 1, 14, 12, 0, 0),
            Count = 2,
            RepeatEvery = TimeSpan.FromHours(1),
            Span = new DateTimeSpan
            {
                Time = new TimeOnly(12, 0),
                DurationTime = TimeSpan.FromHours(1)
            }
        };
        dates = repeatable.GetRepeats().ToArray();
        Assert.Equal(2, dates.Length);
        Assert.Equal(new DateTime(2023, 1, 1, 13, 0, 0), dates[0].Start);
        Assert.Equal(new DateTime(2023, 1, 1, 14, 0, 0), dates[0].End);
        Assert.Equal(new DateTime(2023, 1, 1, 14, 0, 0), dates[1].Start);
        Assert.Equal(new DateTime(2023, 1, 1, 15, 0, 0), dates[1].End);

        repeatable = new DateTimeRepeats
        {
            StartDate = new DateOnly(2023, 1, 1),
            Count = 4,
            Span = new DateTimeSpan
            {
                Time = new TimeOnly(12, 0),
                DaysOfMonth = new() { 7, 5, 1, 7 },
                DurationTime = TimeSpan.FromHours(1)
            }
        };
        dates = repeatable.GetRepeats().ToArray();
        Assert.Equal(4, dates.Length);
        Assert.Equal(new DateTime(2023, 1, 5, 12, 0, 0), dates[0].Start);
        Assert.Equal(new DateTime(2023, 1, 5, 13, 0, 0), dates[0].End);
        Assert.Equal(new DateTime(2023, 1, 7, 12, 0, 0), dates[1].Start);
        Assert.Equal(new DateTime(2023, 1, 7, 13, 0, 0), dates[1].End);
        Assert.Equal(new DateTime(2023, 2, 1, 12, 0, 0), dates[2].Start);
        Assert.Equal(new DateTime(2023, 2, 1, 13, 0, 0), dates[2].End);
        Assert.Equal(new DateTime(2023, 2, 5, 12, 0, 0), dates[3].Start);
        Assert.Equal(new DateTime(2023, 2, 5, 13, 0, 0), dates[3].End);

        repeatable = new DateTimeRepeats
        {
            StartDate = new DateOnly(2023, 1, 1),
            Count = 2,
            RepeatEvery = TimeSpan.FromMinutes(60),
            Span = new DateTimeSpan
            {
                Time = new TimeOnly(12, 15),
                DurationTime = TimeSpan.FromMinutes(45)
            }
        };
        dates = repeatable.GetRepeats().ToArray();
        Assert.Equal(2, dates.Length);
        Assert.Equal(new DateTime(2023, 1, 1, 13, 15, 0), dates[0].Start);
        Assert.Equal(new DateTime(2023, 1, 1, 14, 0, 0), dates[0].End);
        Assert.Equal(new DateTime(2023, 1, 1, 14, 15, 0), dates[1].Start);
        Assert.Equal(new DateTime(2023, 1, 1, 15, 0, 0), dates[1].End);

        repeatable = new DateTimeRepeats
        {
            StartDate = new DateOnly(2023, 1, 1),
            Count = 2,
            RepeatEvery = TimeSpan.FromMinutes(45),
            Span = new DateTimeSpan
            {
                Time = new TimeOnly(12, 15),
                DurationTime = TimeSpan.FromMinutes(45)
            }
        };
        dates = repeatable.GetRepeats().ToArray();
        Assert.Equal(2, dates.Length);
        Assert.Equal(new DateTime(2023, 1, 1, 13, 0, 0), dates[0].Start);
        Assert.Equal(new DateTime(2023, 1, 1, 13, 45, 0), dates[0].End);
        Assert.Equal(new DateTime(2023, 1, 1, 13, 45, 0), dates[1].Start);
        Assert.Equal(new DateTime(2023, 1, 1, 14, 30, 0), dates[1].End);
    }
}
