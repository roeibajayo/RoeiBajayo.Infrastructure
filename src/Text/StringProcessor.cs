using System;
using System.Collections.Generic;
using System.Linq;

namespace RoeiBajayo.Infrastructure.Text;

public class StringProcessor
{
    private readonly static char[] _trimChars = [' ', '\t', '\n', '\r'];

    internal int startIndex = 0;
    internal int endIndex = 0;
    internal string text = string.Empty;

    public string Text => text;
    public int Length =>
        endIndex - startIndex;

    public StringProcessor() : this(string.Empty) { }
    public StringProcessor(string text)
    {
        SetText(text);
    }
    public StringProcessor(StringProcessor copyFrom)
    {
        CopyFrom(copyFrom);
    }

    protected void SetText(string value)
    {
        startIndex = 0;
        text = value ?? string.Empty;
        endIndex = text.Length;
    }

    public void Clear()
    {
        SetText(string.Empty);
    }
    public void CopyFrom(StringProcessor original)
    {
        startIndex = original.startIndex;
        endIndex = original.endIndex;
        text = original.text;
    }
    public void CopyTo(StringProcessor destination)
    {
        destination.startIndex = startIndex;
        destination.endIndex = endIndex;
        destination.text = text;
    }

    public void Append(string input)
    {
        if (string.IsNullOrEmpty(input))
            return;

        SetText(string.Concat(AsSpan(), input));
    }
    public void Prepend(string input)
    {
        if (string.IsNullOrEmpty(input))
            return;

        SetText(string.Concat(input, AsSpan()));
    }
    public StringProcessor Replace(string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(oldValue))
            return new StringProcessor(this);

        return new StringProcessor(ToString().Replace(oldValue, newValue));
    }

    public int IndexOf(char input) =>
        IndexOf(input, 0);
    public int IndexOf(char input, int startIndex) =>
        IndexOf(input, startIndex, Length - startIndex);
    public int IndexOf(char input, int startIndex, int count)
    {
        var result = AsSpan(startIndex, count).IndexOf(input);
        return result == -1 ? -1 : result + startIndex;
    }

    public int IndexOf(string input) =>
        IndexOf(input, 0);
    public int IndexOf(string input, int startIndex) =>
        IndexOf(input, startIndex, Length - startIndex, StringComparison.CurrentCulture);
    public int IndexOf(string input, int startIndex, StringComparison stringComparison) =>
        IndexOf(input, startIndex, Length - startIndex, stringComparison);
    public int IndexOf(string input, int startIndex, int count) =>
        IndexOf(input, startIndex, count, StringComparison.CurrentCulture);
    public int IndexOf(string input, int startIndex, int count, StringComparison stringComparison)
    {
        var result = AsSpan(startIndex, count).IndexOf(input, stringComparison);
        return result == -1 ? -1 : result + startIndex;
    }

    public int IndexOfAny(params char[] anyOf) =>
        IndexOfAny(0, anyOf);
    public int IndexOfAny(int startIndex, params char[] anyOf) =>
        IndexOfAny(startIndex, Length - startIndex, anyOf);
    public int IndexOfAny(int startIndex, int count, params char[] anyOf)
    {
        var result = AsSpan(startIndex, count).IndexOfAny(anyOf);
        return result == -1 ? -1 : result + startIndex;
    }

    public int LastIndexOf(string input) =>
        LastIndexOf(input, Length - 1);
    public int LastIndexOf(string input, int startIndex) =>
        LastIndexOf(input, startIndex, StringComparison.CurrentCulture);
    public int LastIndexOf(string input, int startIndex, StringComparison stringComparison)
    {
        var result = AsSpan(0, startIndex).LastIndexOf(input, stringComparison);
        return result == -1 ? -1 : result;
    }

    public int LastIndexOf(char input) =>
        LastIndexOf(input, Length - 1);
    public int LastIndexOf(char input, int startIndex)
    {
        var result = AsSpan(0, startIndex).LastIndexOf(input);
        return result == -1 ? -1 : result;
    }

    public bool StartsWith(char input)
    {
        if (Length == 0 || startIndex + 1 >= endIndex)
            return false;

        return this[0] == input;
    }
    public bool StartsWith(string input) =>
        StartsWith(input, 0);
    public bool StartsWith(string input, int startIndex)
    {
        if (this.startIndex + startIndex + input.Length >= endIndex)
            return false;

        var span = AsSpan(startIndex, input.Length);
        return span.SequenceEqual(input);
    }

    public bool EndsWith(char input)
    {
        if (Length == 0 || endIndex + 1 < startIndex)
            return false;

        return this[Length - 1] == input;
    }
    public bool EndsWith(string input)
    {
        if (Length == 0 || endIndex + input.Length < startIndex)
            return false;

        var span = AsSpan(Length - input.Length);
        return span.SequenceEqual(input);
    }

    public string Substring(int startIndex, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(startIndex);

        if (count < 0 || startIndex + count > Length)
            throw new ArgumentOutOfRangeException(nameof(count));

        return text!.Substring(this.startIndex + startIndex, count);
    }
    public string Substring(int startIndex) =>
        Substring(startIndex, Length - startIndex);

    public StringProcessor Cut(int fromIndex, int toIndex)
    {
        if (fromIndex < 0)
        {
            if (Length - fromIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(fromIndex));

            return Cut(Length + fromIndex, toIndex);
        }

        if (toIndex < 0)
        {
            if (Length - toIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(toIndex));

            return Cut(fromIndex, Length + toIndex);
        }

        var result = new StringProcessor(this);

        if (fromIndex < 0 || fromIndex > toIndex)
            throw new ArgumentOutOfRangeException(nameof(fromIndex));

        if (toIndex - fromIndex > Length)
            throw new ArgumentOutOfRangeException(nameof(toIndex));

        result.endIndex = startIndex + toIndex;
        result.startIndex += fromIndex;

        return result;
    }
    public StringProcessor CutFrom(int fromIndex) =>
        Cut(fromIndex, Length);
    public StringProcessor CutTo(int toIndex) =>
        Cut(0, toIndex);

    public StringProcessor Trim() =>
        Trim(_trimChars);
    public StringProcessor Trim(params char[] chars) =>
        TrimStart(chars).TrimEnd(chars);
    public StringProcessor TrimStart() =>
        TrimStart(_trimChars);
    public StringProcessor TrimStart(params char[] chars)
    {
        ArgumentNullException.ThrowIfNull(chars);

        if (chars.Length == 0)
            throw new ArgumentException(null, nameof(chars));

        ArgumentNullException.ThrowIfNull(Text);

        var length = Length;
        if (length == 0)
            return new StringProcessor(this);

        var span = AsSpan();
        var charsSpan = chars.AsSpan();
        var newStartIndex = 0;

        for (; newStartIndex <= length; newStartIndex++)
        {
            if (charsSpan.IndexOf(span[newStartIndex]) == -1)
                break;
        }

        if (newStartIndex != 0)
        {
            return CutFrom(newStartIndex);
        }

        return new StringProcessor(this);
    }
    public StringProcessor TrimEnd() =>
        TrimEnd(_trimChars);
    public StringProcessor TrimEnd(params char[] chars)
    {
        ArgumentNullException.ThrowIfNull(chars);

        if (chars.Length == 0)
            throw new ArgumentException(null, nameof(chars));

        ArgumentNullException.ThrowIfNull(Text);

        if (Length == 0)
            return new StringProcessor(this);

        var span = AsSpan();
        var charsSpan = chars.AsSpan();
        var newEndIndex = Length - 1;

        for (; newEndIndex >= 0; newEndIndex--)
        {
            if (charsSpan.IndexOf(span[newEndIndex]) == -1)
                break;
        }

        if (newEndIndex != Length - 1)
        {
            return CutTo(newEndIndex + 1);
        }

        return new StringProcessor(this);
    }

    public string GetRange(int fromIndex, int toIndex) =>
        Cut(fromIndex, toIndex).ToString();
    public string GetRangeFrom(int fromIndex) =>
        GetRange(fromIndex, Length);
    public string GetRangeTo(int toIndex) =>
        GetRange(0, toIndex);

    public IEnumerable<StringProcessor> Split(string seperator) =>
        Split(seperator, StringSplitOptions.None);
    public IEnumerable<StringProcessor> Split(string seperator, StringSplitOptions stringSplitOptions) =>
        SplitInternal(seperator, stringSplitOptions);
    public IEnumerable<StringProcessor> Split(char seperator) =>
        Split(seperator, StringSplitOptions.None);
    public IEnumerable<StringProcessor> Split(char seperator, StringSplitOptions stringSplitOptions) =>
        SplitInternal(seperator, stringSplitOptions);
    public IEnumerable<StringProcessor> Split(char[] seperators) =>
        Split(seperators, StringSplitOptions.None);
    public IEnumerable<StringProcessor> Split(char[] seperators, StringSplitOptions stringSplitOptions) =>
        SplitInternal(seperators, stringSplitOptions);
    private IEnumerable<StringProcessor> SplitInternal(char seperator, StringSplitOptions stringSplitOptions)
    {
        if (Length == 0)
        {
            if ((stringSplitOptions & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries)
                yield return Empty;
            yield break;
        }

        var span = Text;
        var endIndex = Length;
        var from = 0;
        for (var i = startIndex; i < endIndex; i++)
        {
            if (span[i] == seperator)
            {
                if (from == i)
                {
                    if ((stringSplitOptions & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries)
                        yield return Empty;
                }
                else
                {
                    var next = Cut(from - startIndex, i);

                    if ((stringSplitOptions & StringSplitOptions.TrimEntries) == StringSplitOptions.TrimEntries)
                        yield return next.Trim();
                    else if ((stringSplitOptions & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries || !next.IsNullOrWhiteSpace())
                        yield return next;
                }

                from = i + 1;
            }
        }

        if (from == endIndex)
        {
            if ((stringSplitOptions & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries)
                yield return Empty;
        }
        else
        {
            var next = Cut(from - startIndex, endIndex);
            if ((stringSplitOptions & StringSplitOptions.TrimEntries) == StringSplitOptions.TrimEntries)
                yield return next.Trim();
            else if ((stringSplitOptions & StringSplitOptions.RemoveEmptyEntries) != StringSplitOptions.RemoveEmptyEntries || !next.IsNullOrWhiteSpace())
                yield return next;
        }
    }
    private IEnumerable<StringProcessor> SplitInternal(char[] seperators, StringSplitOptions stringSplitOptions)
    {
        ArgumentNullException.ThrowIfNull(seperators);

        if (Length == 0)
        {
            if (stringSplitOptions != StringSplitOptions.RemoveEmptyEntries)
                yield return Empty;
            yield break;
        }

        var span = Text;
        var endIndex = Length;
        var from = 0;
        Array.Sort(seperators);
        var fromChar = seperators[0];
        var isBetween = (uint)(seperators[^1] - fromChar);
        for (var i = startIndex; i < endIndex; i++)
        {
            if ((uint)(span[i] - fromChar) <= isBetween)
            {
                for (var j = 0; j < seperators.Length; j++)
                {
                    if (seperators[j] == span[i])
                    {
                        if (from == i)
                        {
                            if (stringSplitOptions != StringSplitOptions.RemoveEmptyEntries)
                                yield return Empty;
                        }
                        else
                        {
                            var next = Cut(from - startIndex, i);

                            if (stringSplitOptions == StringSplitOptions.TrimEntries)
                                yield return next.Trim();
                            else if (stringSplitOptions != StringSplitOptions.RemoveEmptyEntries || !next.IsNullOrWhiteSpace())
                                yield return next;
                        }

                        from = i + 1;

                        break;
                    }
                }
            }
        }

        if (from == endIndex)
        {
            if (stringSplitOptions != StringSplitOptions.RemoveEmptyEntries)
                yield return Empty;
        }
        else
        {
            var next = Cut(from - startIndex, endIndex);
            if (stringSplitOptions == StringSplitOptions.TrimEntries)
                yield return next.Trim();
            else if (stringSplitOptions != StringSplitOptions.RemoveEmptyEntries || !next.IsNullOrWhiteSpace())
                yield return next;
        }
    }
    private IEnumerable<StringProcessor> SplitInternal(string seperator, StringSplitOptions stringSplitOptions)
    {
        ArgumentNullException.ThrowIfNull(Text);
        ArgumentException.ThrowIfNullOrEmpty(seperator);

        if (Length == 0)
        {
            if (stringSplitOptions != StringSplitOptions.RemoveEmptyEntries)
                yield return Empty;

            yield break;
        }

        var from = 0;
        var endIndex = Length;
        int indexOf;

        while (from != endIndex && (indexOf = IndexOf(seperator, from)) != -1)
        {
            if (from == indexOf)
            {
                if (stringSplitOptions != StringSplitOptions.RemoveEmptyEntries)
                    yield return Empty;
            }
            else
            {
                var next = Cut(from - startIndex, indexOf);

                if (stringSplitOptions == StringSplitOptions.TrimEntries)
                    yield return next.Trim();
                else if (stringSplitOptions != StringSplitOptions.RemoveEmptyEntries || !next.IsNullOrWhiteSpace())
                    yield return next;
            }

            from = indexOf + seperator.Length;
        }

        if (from == endIndex)
        {
            if (stringSplitOptions != StringSplitOptions.RemoveEmptyEntries)
                yield return Empty;
            yield break;
        }
        else
        {
            var next = Cut(from, endIndex);
            if (stringSplitOptions == StringSplitOptions.TrimEntries)
                yield return next.Trim();
            else if (stringSplitOptions != StringSplitOptions.RemoveEmptyEntries || !next.IsNullOrWhiteSpace())
                yield return next;
        }
    }

    public StringProcessor GetTextBetween(string from, string to,
        int getResultIndex = 0, bool reverse = false, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        foreach (var result in GetAllTextBetween(from, to, reverse: reverse, stringComparison: stringComparison))
        {
            if (getResultIndex == 0)
                return result;

            getResultIndex--;
        }

        return Empty;
    }
    public IEnumerable<StringProcessor> GetAllTextBetween(string from, string to,
        bool reverse = false, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        if (string.IsNullOrEmpty(from))
            throw new ArgumentNullException(nameof(from));

        if (string.IsNullOrEmpty(to))
            throw new ArgumentNullException(nameof(to));

        var indexOf = reverse ? LastIndexOf(from, 0, stringComparison) : IndexOf(from, 0, stringComparison);

        while (indexOf != -1)
        {
            var start = indexOf + from.Length;
            var end = IndexOf(to, start, stringComparison);

            if (end == -1 || start >= end)
                yield break;

            yield return Cut(start, end);

            indexOf = reverse ? LastIndexOf(from, indexOf, stringComparison) : IndexOf(from, end + to.Length, stringComparison);
        }
    }

    public bool Contains(string input) =>
        IndexOf(input) != -1;
    public bool Contains(char input) =>
        IndexOf(input) != -1;

    public bool IsNullOrWhiteSpace()
    {
        if (Length == 0)
            return true;

        return Trim().Length == 0;
    }
    public bool IsNullOrEmpty()
    {
        return Length == 0;
    }

    public char this[int index]
    {
        get
        {
            return Text[startIndex + index];
        }
    }

    public ReadOnlySpan<char> AsSpan() =>
        Text.AsSpan(startIndex, Length);
    public ReadOnlySpan<char> AsSpan(int startIndex) =>
        Text.AsSpan(this.startIndex + startIndex, Length - startIndex);
    public ReadOnlySpan<char> AsSpan(int startIndex, int length) =>
        Text.AsSpan(this.startIndex + startIndex, length);

    public override string ToString()
    {
        if (Length != 0 && text.Length != Length)
            SetText(new string(AsSpan()));

        return text;
    }
    public override int GetHashCode() =>
        ToString().GetHashCode();
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj is string str)
            return str.Length == Length && str == ToString();

        if (obj is StringProcessor sp)
            return sp.Length == Length && sp.ToString() == ToString();

        return base.Equals(obj);
    }

    public bool Equals(string str, StringComparison stringComparison)
    {
        if (str is null)
            return false;

        if (str.Length != Length)
            return false;

        return str.Equals(ToString(), stringComparison);
    }
    public bool Equals(StringProcessor value, StringComparison stringComparison)
    {
        if (value is null)
            return false;

        var str = value.ToString();

        if (str.Length != Length)
            return false;

        return str.Equals(ToString(), stringComparison);
    }

    #region operators

    public static implicit operator string(StringProcessor value) =>
        value.ToString();
    public static implicit operator StringProcessor(string value) =>
        new(value);

    public static StringProcessor operator +(StringProcessor left, StringProcessor right)
    {
        left.Append(right);
        return left;
    }
    public static bool operator ==(StringProcessor left, StringProcessor right) =>
        left.Equals(right);
    public static bool operator !=(StringProcessor left, StringProcessor right) =>
        !left.Equals(right);

    #endregion

    public static StringProcessor Empty => new();
}
