using Infrastructure.Utils.Text.Analyzers.FrequencyAnalyzerStores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Infrastructure.Utils.Text.Analyzers;

/// <summary>
/// Used to analyze the frequency of words or phrases within a given input of sentences.
/// Add a lot of sentences with Add function.
/// Fix shorten words: CreateShortenWord("ef", "EntityFramework").
/// Group words: CreateGroup("EntityFramework", new[] { "EntityFramework Core", "Entity Framework" }).
/// Get segments of sentence based on your collection: GetSegments("bla bla ef bla bla") => ["EntityFramework"]).
/// Use Optimaze() often.
/// </summary>
public class FrequencyAnalyzer(IFrequencyAnalyzerStore store) : IDisposable
{
    public int MinSegmentWords = 2;
    public int MaxSegmentWords = 5;
    public int MinLettersForWord = 2;
    public readonly HashSet<string> IgnoredWords = [];
    public readonly List<string> IgnoredSegmentsContains = [];
    //public readonly List<string> IgnoredSegmentsStartsWith = new() { "לא", "של", "על", "את" };
    //public readonly List<string> IgnoredSegmentsEndsWith = new() { "לא", "של", "על", "את" };
    public readonly List<string> IgnoredSegmentsStartsWith = [];
    public readonly List<string> IgnoredSegmentsEndsWith = [];
    private readonly static char[] WordPrefixes = ['ו', 'ה', 'ל', 'ב', 'ש', 'כ', 'מ'];

    public FrequencyAnalyzer() : this(new MemoryStore()) { }

    public void Add(string sentence)
    {
        if (sentence == null)
            return;

        if (MinSegmentWords > 1 && !sentence.Contains(' '))
            return;

        foreach (var s in sentence.Split(['.', ',', '!', '?', ':', '|', ']', '[', '(', ')', '\r', '\n']))
            AddSentence(s);
    }
    public void Add(IEnumerable<string> sentences) =>
        sentences.ForEach(Add);

    private void AddSentence(string sentence)
    {
        if (MinSegmentWords > 1 && !sentence.Contains(' '))
            return;

        var words = GetWords(sentence);
        if (words.Length >= MinSegmentWords)
        {
            for (var i = 0; i <= words.Length - MinSegmentWords; i++)
            {
                foreach (var segment in GetSegments(words, i).Reverse())
                {
                    if (IsInvalidSegment(segment))
                        continue;

                    var fixedSegment = segment;
                    while (fixedSegment.Length - 1 >= MinLettersForWord && WordPrefixes.Any(x => fixedSegment.StartsWith(x)))
                    {
                        fixedSegment = fixedSegment[1..];

                        if (IsInvalidSegment(fixedSegment))
                            store.CountIfExists(fixedSegment);
                    }

                    if (!store.TryCountOne(segment))
                    {
                        i += segment.Count(x => x == ' ');
                        break;
                    }
                }
            }
        }
    }
    private string[] GetWords(string sentence)
    {
        var shortens = store.GetShortenWords();
        return SplitWords(sentence)
            .Where(x => x != null &&
                x.Length >= MinLettersForWord &&
                (IgnoredWords == null || !IgnoredWords.Contains(x.ToLower())))
            .Select(x => shortens.GetValueOrDefault(x, x))
            .ToArray();
    }
    private static IEnumerable<string> SplitWords(string sentence)
    {
        var result = new StringBuilder();
        foreach (var c in sentence)
        {
            if (c == '\'' || c == '\"' ||
                c == '-' || c == '־'
                || c >= 1488 && c <= 1514//hebrew
                || c >= 65 && c <= 97 //Aa
                || c >= 90 && c <= 122 //Zz
                )
            {
                if (c != '\'' && c != '\"')
                    result.Append(c == '־' || c == '-' ? ' ' : c);
            }
            else if (result.Length != 0)
            {
                yield return result.ToString().Trim().ToLower();
                result.Clear();
            }
        }

        if (result.Length > 0)
            yield return result.ToString().Trim().ToLower();
    }
    protected virtual bool IsInvalidSegment(string segment) =>
        IgnoredSegmentsContains.Any(x => segment.Contains(x)) ||
        IgnoredSegmentsStartsWith.Any(x => segment.StartsWith(x + " ")) ||
        IgnoredSegmentsEndsWith.Any(x => segment.EndsWith(" " + x));
    private IEnumerable<string> GetSegments(string[] words, int index)
    {
        if (words.Length - index < MinSegmentWords)
            yield break;

        var sb = new StringBuilder();

        for (var i = index; i - index < MinSegmentWords; i++)
        {
            if (sb.Length != 0)
                sb.Append(' ');
            sb.Append(words[i]);
        }
        yield return sb.ToString();

        for (var i = MinSegmentWords + index; i < words.Length && i - index < MaxSegmentWords; i++)
        {
            sb.Append(' ');
            sb.Append(words[i]);
            yield return sb.ToString();
        }
    }

    public void CreateGroup(string key, IEnumerable<string> keys)
    {
        store.CreateGroup(key, keys);
    }
    public void CreateShortenWord(string shorten, string key)
    {
        store.CreateShortenWord(shorten, key);
    }

    public IReadOnlyDictionary<string, int> Get(int minimumCounter = 3) =>
        store.Get(minimumCounter);
    public IEnumerable<string> GetSegments(string sentence) =>
        GetAllSegments(sentence).Distinct();
    private IEnumerable<string> GetAllSegments(string sentence)
    {
        var keys = Get();
        var groups = store.GetGroups();
        var words = GetWords(sentence);
        if (words.Length >= MinSegmentWords)
        {
            for (var i = 0; i <= words.Length - MinSegmentWords; i++)
            {
                foreach (var segment in GetSegments(words, i).Reverse())
                {
                    if (IsInvalidSegment(segment))
                        continue;

                    var fixedSegment = segment;
                    while (fixedSegment.Length - 1 >= MinLettersForWord && WordPrefixes.Any(x => fixedSegment.StartsWith(x)))
                    {
                        fixedSegment = fixedSegment[1..];
                        fixedSegment = groups.GetValueOrDefault(fixedSegment, fixedSegment);
                        if (keys.ContainsKey(fixedSegment))
                            yield return segment;
                    }

                    fixedSegment = groups.GetValueOrDefault(segment, segment);
                    if (keys.ContainsKey(fixedSegment))
                    {
                        yield return fixedSegment;
                        i += segment.Count(x => x == ' ');
                        break;
                    }
                }
            }
        }
    }
    public void Optimaze(double ratio = 0.1)
    {
        var items = store.Get();
        var with = new Dictionary<string, int>();

        var groups = store.GetGroups();
        var groupedBySimilar = items.GroupBy(x => x.Key.TrimStart(WordPrefixes));
        foreach (var group in groupedBySimilar)
        {
            var key = group.MaxBy(x => x.Value).Key;
            key = groups.GetValueOrDefault(key, key);
            var value = group.Sum(x => x.Value);
            if (!with.TryAdd(key, value))
            {
                with[key] += value;
            }
        }

        var values = with.Values.OrderBy(x => x).ToArray();
        var minValue = values.Skip((int)(values.Length * ratio)).FirstOrDefault();
        if (minValue > 0)
        {
            var removes = with.Where(x => x.Value <= minValue).Select(x => x.Key).ToArray();
            for (var i = 0; i < removes.Length; i++)
                with.Remove(removes[i]);
        }

        store.Replace(with);
    }

    public void Clear() =>
        store.Clear();

    public void Dispose()
    { 
        GC.SuppressFinalize(this);
        store.Dispose();
    }

}
