using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace RoeiBajayo.Infrastructure.Hebrew;

public static class HebrewExtensions
{
    public static string? ToHebrewLetters(this int number)
    {
        if (number <= 0)
            return null;

        if (number <= 10 || (number <= 100 && number % 10 == 0))
            return GetChar(number).ToString();

        var sb = new StringBuilder();

        if (number > 1000)
        {
            sb.Append(GetChar(number / 1000));
            sb.Append('\'');
            number %= 1000;
        }

        while (number > 400)
        {
            sb.Append(GetChar(400));
            number -= 400;
        }

        if (number >= 300)
        {
            sb.Append(GetChar(300));
            number -= 300;
        }

        if (number >= 200)
        {
            sb.Append(GetChar(200));
            number -= 200;
        }

        if (number >= 100)
        {
            sb.Append(GetChar(100));
            number -= 100;
        }

        if (number == 15)
        {
            sb.Append(GetChar(9));
            sb.Append(GetChar(6));
            return sb.ToString();
        }

        if (number == 16)
        {
            sb.Append(GetChar(9));
            sb.Append(GetChar(7));
            return sb.ToString();
        }

        if (number >= 10)
        {
            sb.Append(GetChar(number / 10 * 10));
            number %= 10;
        }

        if (number > 0)
        {
            sb.Append(GetChar(number));
        }

        return sb.ToString();

        static char GetChar(int number) =>
            number switch
            {
                1 => 'א',
                2 => 'ב',
                3 => 'ג',
                4 => 'ד',
                5 => 'ה',
                6 => 'ו',
                7 => 'ז',
                8 => 'ח',
                9 => 'ט',
                10 => 'י',
                20 => 'כ',
                30 => 'ל',
                40 => 'מ',
                50 => 'נ',
                60 => 'ס',
                70 => 'ע',
                80 => 'פ',
                90 => 'צ',
                100 => 'ק',
                200 => 'ר',
                300 => 'ש',
                400 => 'ת',
                _ => throw new ArgumentOutOfRangeException()
            };
    }

    public static bool IsHebrewLetter(this char c)
    {
        return c >= 'א' && c <= 'ת';
    }

    public static bool IsHebrewLetterOrNikud(this char c)
    {
        return c.IsHebrewLetter() || c.IsNikudLetter();
    }

    public static bool ContainsNikud(this string text)
    {
        var span = text.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            if (IsNikudLetter(span[i]))
                return true;
        }
        return false;
    }

    public static string CleanNikud(this string text)
    {
        var found = false;
        var span = text.AsSpan();
        var sb = new StringBuilder();
        for (int i = 0; i < span.Length; i++)
        {
            if (!IsNikudLetter(span[i]))
                sb.Append(span[i]);
            else
                found = true;
        }

        if (!found)
            return text;

        return sb.ToString();
    }

    public static bool IsNikudLetter(this char c)
    {
        return c != '־' && c >= 1456 && c <= 1479;
    }
}
