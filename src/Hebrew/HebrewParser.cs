using System;
using System.Collections.Generic;

namespace RoeiBajayo.Infrastructure.Hebrew;

public class HebrewParser
{
    private static readonly HashSet<string> PlaceholdersWord = ["סימן", "פרק", "פסוק", "אות", "פרשת", "דף"];

    private readonly List<ParsedWord> result = [];
    private List<ParsedWord> lastSentance = [];

    private void AddWord(int index, string text, WordType type)
    {
        var word = new ParsedWord(index, text, GetWordType(text, type));
        result.Add(word);
        lastSentance.Add(word);

        if (type == WordType.EndSentence)
            StartNewSentance();
    }

    private WordType GetWordType(string word, WordType type)
    {
        if (type is not WordType.Hebrew and not WordType.Kizur and not WordType.Initials and not WordType.Unknown)
            return type;

        if (word.Length == 1 ||
            (word.Length == 2 && type == WordType.Kizur))
        {
            var first = word[0];
            if (first.IsHebrewLetter() && first != 'פ' && (first < 'ג' || first > 'ה'))
            {
                return WordType.HebrewNumber;
            }
        }

        if (IsPlaceholderWord(word))
            return WordType.PlaceHolder;

        if (lastSentance.Count > 1 &&
            lastSentance[^1].Type == WordType.Space &&
            lastSentance[^2].Type == WordType.PlaceHolder)
        {
            if (type is WordType.Kizur or WordType.Initials || IsPosibleNumber(word))
                return WordType.HebrewNumber;
        }

        if (type is not WordType.Kizur and not WordType.Initials && IsAramicWord(word))
            return WordType.Aramic;

        return type;
    }

    private void StartNewSentance()
    {
        lastSentance = [];
    }

    public List<ParsedWord> Parse(string text)
    {
        var span = text.AsSpan();
        var start = -1;
        var type = WordType.Unknown;
        for (int i = 0; i < span.Length; i++)
        {
            var c = span[i];

            if (c == ' ')
            {
                if (start != -1 && start != i)
                {
                    AddWord(start, span[start..i].ToString(), type);
                }

                start = i;
                while (i + 1 < span.Length && span[i + 1] == ' ')
                    i++;

                AddWord(start, span[start..(i + 1)].ToString(), WordType.Space);
                start = i + 1;
                type = WordType.Unknown;
                continue;
            }

            if (c.IsHebrewLetterOrNikud())
            {
                if (type == WordType.Unknown)
                {
                    if (start != -1 && start != i)
                    {
                        AddWord(start, span[start..i].ToString(), type);
                        start = i;
                    }

                    type = WordType.Hebrew;
                }

                if (start != -1)
                    continue;

                start = i;
                continue;
            }


            if (c is '(' or '{' or '[')
            {
                if (start != -1 && start != i)
                {
                    AddWord(start, span[start..i].ToString(), type);
                    start = i;
                }

                AddWord(start, span.Slice(i, 1).ToString(), WordType.StartRemark);
                start = i + 1;
                type = WordType.Unknown;
                continue;
            }

            if (c is ')' or '}' or ']')
            {
                if (start != -1 && start != i)
                {
                    AddWord(start, span[start..i].ToString(), type);
                    start = i;
                }

                AddWord(start, span.Slice(i, 1).ToString(), WordType.EndRemark);
                start = i + 1;
                type = WordType.Unknown;
                continue;
            }

            if (c == '.')
            {
                if (start != -1 && start != i)
                {
                    AddWord(start, span[start..i].ToString(), type);
                    start = i;
                }

                AddWord(start, span.Slice(i, 1).ToString(), WordType.EndSentence);
                start = i + 1;
                type = WordType.Unknown;
                continue;
            }

            if (c is ',' or ':' or '׃' or ';' or '-' or '!' or '?')
            {
                if (start != -1 && start != i)
                {
                    AddWord(start, span[start..i].ToString(), type);
                    start = i;
                }

                AddWord(start, span.Slice(i, 1).ToString(), WordType.Punctuation);
                start = i + 1;
                type = WordType.Unknown;
                continue;
            }

            if (c is '״' or '\"')
            {
                if (i > 0 &&
                    i + 1 < span.Length &&
                    span[i - 1].IsHebrewLetterOrNikud() &&
                    span[i + 1].IsHebrewLetter() &&
                    (i + 1 == span.Length || (i + 2 < span.Length && !span[i + 2].IsHebrewLetterOrNikud())))
                {
                    type = WordType.Initials;
                    continue;
                }

                if (i == 0 || !span[i - 1].IsHebrewLetterOrNikud())
                {
                    if (start == -1)
                    {
                        AddWord(start, span[..(i + 1)].ToString(), WordType.StartQuote);
                    }
                    else
                    {
                        AddWord(start, span[start..i].ToString(), type);
                        AddWord(start, span.Slice(i, 1).ToString(), WordType.StartQuote);
                    }

                    start = i + 1;
                    type = WordType.Unknown;
                    continue;
                }

                if (i > 0 &&
                    i + 1 < span.Length &&
                    !span[i + 1].IsHebrewLetter())
                {
                    if (start == -1)
                    {
                        AddWord(start, span[..(i + 1 - start)].ToString(), WordType.EndQuote);
                    }
                    else
                    {
                        AddWord(start, span[start..i].ToString(), type);
                        AddWord(start, span.Slice(i, 1).ToString(), WordType.EndQuote);
                    }

                    start = i + 1;
                    type = WordType.Unknown;
                    continue;
                }
            }

            if (c == '\'')
            {
                if (i > 0 &&
                    i + 2 < span.Length &&
                    span[i - 1].IsHebrewLetterOrNikud() &&
                    span[i + 1] == '\'' &&
                    span[i + 2].IsHebrewLetterOrNikud())
                {
                    type = WordType.Initials;
                    i++;
                    continue;
                }
            }

            if (type == WordType.Hebrew)
            {
                if (c == '\'')
                {
                    if (i != span.Length - 1)
                    {
                        if (span[i + 1].IsHebrewLetter()) // א'הבה
                        {
                            type = WordType.Initials;
                            continue;
                        }
                    }

                    AddWord(start, span[start..(i + 1)].ToString(), WordType.Kizur);

                    start = i + 1;
                    type = WordType.Unknown;
                    continue;
                }
            }

            if (type == WordType.Hebrew || type == WordType.Initials)
            {
                AddWord(start, span[start..i].ToString(), type);
                type = WordType.Unknown;
                start = i;
                i--;
                continue;
            }
        }

        if (start != -1 && span.Length - start > 1)
            AddWord(start, span[start..].ToString(), type);

        return result;
    }

    private static bool IsMatch(string word, HashSet<string> options)
    {
        if (word.Length < 2)
            return false;

        var span = word.AsSpan();
        var from = span[0] == 'ו' ? 1 : 0;

        if (span[from] is 'ב' or 'ל' or 'ה' or 'ד' or 'כ' or 'מ')
        {
            if (IsMatch(span[(from + 1)..].ToString(), options))
                return true;
        }

        if (from != 0)
        {
            if (IsMatch(span[from..].ToString(), options))
                return true;
        }

        return options.Contains(word);
    }

    private static bool IsPlaceholderWord(string word) =>
        IsMatch(word, PlaceholdersWord);

    private static bool IsPosibleNumber(string word)
    {
        if (word.Length == 0)
            return false;

        if (word.Length == 1)
            return true;

        if (word[^1] == '\'')
            return true;

        var span = word.AsSpan();
        var current = span[0];
        for (var i = 1; i < span.Length; i++)
        {
            if (current <= span[i])
                return false;

            current = span[i];
        }
        return true;
    }

    private static bool IsAramicWord(string word)
    {
        if (word.Length == 1)
            return false;

        var span = word.AsSpan();

        if (span.Length == 2 &&
            ((span[0] == 'ג' && span[1] == 'ו') ||
            (span[0] == 'ב' && span[1] == 'ר')))
            return true;

        if (span.Length <= 2)
            return false;

        if (span[^1] == 'א')
        {
            if (span[0] == 'ד')
                return true;

            if (span.Length == 3 && span[1] == 'ל' && span[0] == 'א')
                return false;

            if (span.Length < 3 || span[^3] != 'ה' || (span[^2] != 'ו' && span[^2] != 'י'))
                return true;
        }

        if (span.Length > 3 && span[^1] == 'ן' && span[^2] == 'י' && span[^3] != 'י')
            return true;

        return false;
    }

    //private string? GetShoresh(string word)
    //{
    //    var span = word.AsSpan();

    //    if (word.Length < 2)
    //        return null;

    //    if (word.Length == 2)
    //        return new string(new char[] { span[0], 'ו', span[1] });

    //    if (word.Length == 3)
    //    {
    //        if (span[1] == 'י')
    //            return new string(new char[] { span[0], 'ו', span[2] });

    //        if (span[2] == 'ת' && (span[1] != 'מ' || span[0] != 'א'))
    //            return new string(new char[] { span[0], 'י', span[1] });

    //        return word;
    //    }

    //    var result = new List<char>(4);
    //    if ()
    //}

}
