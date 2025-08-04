using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace System;

public static partial class StringExtentions
{
#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
    private static readonly Regex containsHebrewRegex = new("[א-ת]{1}", RegexOptions.Compiled);
    private static readonly Regex containsEnglishRegex = new("[a-z]{1}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex startsWithHebrewRegex = new("^[א-ת]{1}", RegexOptions.Compiled);
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
    private static readonly char[] ignoreDoubleChars = ['!', ' ', '*', '#'];
    private static readonly char[] spaceOnlyAfterChars = [':', '!', '.', ','];

    public static string? GetTextBetween(this string input, string from, string to,
        int getResultIndex = 0, bool reverse = false)
    {
        foreach (var result in input.GetAllTextBetween(from, to, reverse: reverse))
        {
            if (getResultIndex == 0)
                return result;

            getResultIndex--;
        }

        return null;
    }
    public static IEnumerable<string> GetAllTextBetween(this string input, string from, string to, bool reverse = false)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentNullException(nameof(input));

        if (string.IsNullOrEmpty(from))
            throw new ArgumentNullException(nameof(from));

        if (string.IsNullOrEmpty(to))
            throw new ArgumentNullException(nameof(to));

        var indexOf = reverse ? input.LastIndexOf(from) : input.IndexOf(from);

        while (indexOf != -1)
        {
            var start = indexOf + from.Length;
            var end = input.IndexOf(to, start);

            if (end == -1 || start >= end)
                yield break;

            yield return input[start..end];

            indexOf = reverse ? input.LastIndexOf(from, indexOf) : input.IndexOf(from, end + to.Length);
        }
    }

    public static string GetCharsOnly(this string input)
    {
        var result = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (c == 32 //space
                || (c >= 1488 && c <= 1514)//hebrew
                || (c >= 65 && c <= 97) //Aa
                || (c >= 90 && c <= 122) //Zz
                )
                result.Append(c);
            else if (c >= 44 && c <= 46)
                result.Append(' ');
        }
        return result.ToString();
    }
    public static bool ContainsHebrew(this string input)
    {
        return containsHebrewRegex.IsMatch(input);
    }
    public static bool ContainsEnglish(this string input)
    {
        return containsEnglishRegex.IsMatch(input);
    }
    public static bool StartsWithHebrew(this string input)
    {
        return startsWithHebrewRegex.IsMatch(input);
    }
    public static string GetHebrewOnly(this string input)
    {
        var result = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (c == 32 //space
                || (c >= 1488 && c <= 1514)//hebrew
                )
                result.Append(c);
            else if (c >= 44 && c <= 46)
                result.Append(' ');
        }
        return result.ToString();
    }
    public static string GetEnglishOnly(this string input)
    {
        var result = new StringBuilder(input.Length);
        foreach (var c in input)
        {
            if (c == 32 //space
                || (c >= 65 && c <= 97) //Aa
                || (c >= 90 && c <= 122) //Zz
                )
                result.Append(c);
            else if (c >= 44 && c <= 46)
                result.Append(' ');
        }
        return result.ToString();
    }

    public static string GetCleanText(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var result = new StringBuilder(input.Length);
        var index = -1;

        for (int i = 0; i < input.Length; i++)
        {
            if (ignoreDoubleChars.Contains(input[i]) && index > -1 && ignoreDoubleChars.Contains(result[index]))
                continue;

            index++;
            result.Append(input[i]);

            if (spaceOnlyAfterChars.Contains(input[i]))
            {
                if (result[index - 1] == ' ')
                {
                    index--;
                    result.Remove(index, 1);
                }
                if (index + 1 < input.Length)
                {
                    index++;
                    result.Append(' ');
                }
            }
        }

        return result.ToString().Trim();
    }

    public static string RemoveBetween(this string input, string fromString, string toString)
    {
        return input.ReplaceBetween(fromString, toString, null);
    }
    public static string RemoveBetween(this string input, int fromIndex, int toIndex)
    {
        return input.ReplaceBetween(fromIndex, toIndex, null);
    }

    public static string ReplaceBetween(this string input, string fromString, string toString,
        string? replace)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(fromString) || string.IsNullOrEmpty(toString))
            return input;

        var fromIndex = input.IndexOf(fromString);
        if (fromIndex < 0)
            return input;

        var toIndex = input.IndexOf(toString, fromIndex);
        if (toIndex < 0)
            return input;

        return input.ReplaceBetween(fromIndex + fromString.Length, toIndex, replace);
    }
    public static string ReplaceBetween(this string input, int fromIndex, int toIndex,
        string? replace)
    {
        if (string.IsNullOrEmpty(input) || fromIndex < 0 || toIndex < 0)
            return input;

        var builder = new StringBuilder(fromIndex + input.Length - toIndex + (replace?.Length ?? 0));
        builder.Append(input[0..fromIndex]);
        builder.Append(replace);
        builder.Append(input, toIndex);
        return builder.ToString();
    }

    public static string ToBase64(this string input, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(input);

        byte[] textAsBytes = (encoding ?? Encoding.UTF8).GetBytes(input);
        return Convert.ToBase64String(textAsBytes);
    }
    public static string FromBase64(this string input, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(input);

        var encoded = Convert.FromBase64String(input);
        return (encoding ?? Encoding.UTF8).GetString(encoded);
    }

    public static string RemoveHtmlTags(this string html, bool saveInnerContent = false)
    {
        if (string.IsNullOrEmpty(html))
            return html;

        if (!saveInnerContent)
        {
            html = RemoveHtmlRegex().Replace(html, "");
        }
        else
        {
            html = RemoveHtmlRegex().Replace(html, match => match.Groups[2].Value);
        }

        return html;
    }

    public static unsafe int GetStaticHashCode(this string value)
    {
        //dont use GetHashCode, it returns different values!

        fixed (char* str = value.ToCharArray())
        {
            char* chPtr = str;
            int num = 0x15051505;
            int num2 = num;
            int* numPtr = (int*)chPtr;
            for (int i = value.Length; i > 0; i -= 4)
            {
                num = (((num << 5) + num) + (num >> 0x1b)) ^ numPtr[0];
                if (i <= 2)
                {
                    break;
                }
                num2 = (((num2 << 5) + num2) + (num2 >> 0x1b)) ^ numPtr[1];
                numPtr += 2;
            }
            var temp = num + (num2 * 0x5d588b65);
            if (temp < 0)
            {
                temp *= -1;
            }
            return temp;
        }
    }

    private static HashSet<char> _InvalidFileNameChars = [];
    public static string RemovePathInvalidChars(this string filepath)
    {
        _InvalidFileNameChars ??= new(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()));

        StringBuilder stringBuilder = new();
        foreach (char c in filepath)
        {
            if (!_InvalidFileNameChars.Contains(c))
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString();
    }

    [GeneratedRegex(@"<[^>]*>(([^<]*)</[^>]*>)?", RegexOptions.Compiled)]
    private static partial Regex RemoveHtmlRegex();
}
