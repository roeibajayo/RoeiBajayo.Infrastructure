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
/// Use Optimize() often.
/// </summary>
public class DeepFrequencyAnalyzer(IDeepStore store) : IDisposable
{
    private readonly static char[] WordPrefixes = ['ו', 'ה', 'ל', 'ב', 'ש', 'כ', 'מ'];

    public bool UseWordPrefixes { get; init; }
    public int MinSegmentWords { get; init; } = 2;
    public int? MaxSegmentWords { get; init; }
    public int MinLettersForWord { get; init; } = 2;
    public List<string> IgnoredSegmentsContains { get; init; } = [];
    public List<string> IgnoredSegmentsStartsWith { get; init; } = [];
    public List<string> IgnoredSegmentsEndsWith { get; init; } = [];

    public DeepFrequencyAnalyzer() : this(new DeepMemoryStore()) { }

    public void Add(string text)
    {
        if (text == null)
            return;

        if (MinSegmentWords > 1 && !text.Contains(' '))
            return;

        foreach (var s in text.Split(['.', ',', '!', '?', ':', '|', ']', '[', '(', ')', '\r', '\n']))
            AddSentence(s);
    }
    public IReadOnlyDictionary<string, int> Get(int minimumCounter = 3) =>
        store.Get(minimumCounter);
    public IEnumerable<string> GetSegments(string sentence) =>
        GetAllSegments(sentence).Distinct();
    private IEnumerable<string> GetAllSegments(string sentence)
    {
        throw new NotImplementedException();
        //var keys = Get();
        //var groups = Store.GetGroups();
        //var words = GetWords(sentence);
        //if (words.Length >= MinSegmentWords)
        //{
        //    for (var i = 0; i <= words.Length - MinSegmentWords; i++)
        //    {
        //        foreach (var segment in GetSegments(words).Reverse())
        //        {
        //            if (IsInvalidSegment(segment))
        //                continue;

        //            var fixedSegment = segment;
        //            while (fixedSegment.Length - 1 >= MinLettersForWord && WordPrefixes.Any(x => fixedSegment.StartsWith(x)))
        //            {
        //                fixedSegment = fixedSegment[1..];
        //                fixedSegment = groups.GetValueOrDefault(fixedSegment, fixedSegment);
        //                if (keys.ContainsKey(fixedSegment))
        //                    yield return segment;
        //            }

        //            fixedSegment = groups.GetValueOrDefault(segment, segment);
        //            if (keys.ContainsKey(fixedSegment))
        //            {
        //                yield return fixedSegment;
        //                i += segment.Count(x => x == ' ');
        //                break;
        //            }
        //        }
        //    }
        //}
    }
    public void Optimize(double ratio = 0.1)
    {
        throw new NotImplementedException();
        //var items = Store.Get();
        //var with = new Dictionary<string, int>();

        //var groups = Store.GetGroups();
        //var groupedBySimilar = items.GroupBy(x => x.Key.TrimStart(WordPrefixes));
        //foreach (var group in groupedBySimilar)
        //{
        //    var key = group.MaxBy(x => x.Value).Key;
        //    key = groups.GetValueOrDefault(key, key);
        //    var value = group.Sum(x => x.Value);
        //    if (!with.TryAdd(key, value))
        //    {
        //        with[key] += value;
        //    }
        //}

        //var values = with.Values.OrderBy(x => x).ToArray();
        //var minValue = values.Skip((int)(values.Length * ratio)).FirstOrDefault();
        //if (minValue > 0)
        //{
        //    var removes = with.Where(x => x.Value <= minValue).Select(x => x.Key).ToArray();
        //    for (var i = 0; i < removes.Length; i++)
        //        with.Remove(removes[i]);
        //}

        //Store.Replace(with);
    }

    private void AddSentence(string sentence)
    {
        var words = GetWords(sentence);
        if (words.Length >= MinSegmentWords)
        {
            var wordsIndex = new List<(int Index, string Word)>();

            for (var i = 0; i <= words.Length - MinSegmentWords; i++)
            {
                var word = words[i];
                var maxMatchWords = 1;
                var editedWord = false;
                var added = new HashSet<string>();

                do
                {
                    if (editedWord)
                        word = word[1..];

                    var sentences = store.GetSentences(word).ToArray();
                    if (sentences.Length > 0)
                    {
                        foreach (var (Sentence, WordIndex) in sentences)
                        {
                            var sb = new StringBuilder(words[i]);
                            var matchWords = 1;
                            var storedSentenceWords = Sentence.Split(' ');
                            for (var j = WordIndex + 1; j < storedSentenceWords.Length && i + matchWords < words.Length; j++)
                            {
                                if ((MaxSegmentWords == null || MaxSegmentWords != matchWords) && storedSentenceWords[j] == words[i + matchWords])
                                {
                                    matchWords++;
                                    sb.Append(' ');
                                    sb.Append(storedSentenceWords[j]);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            if (matchWords >= MinSegmentWords)
                            {
                                var segment = sb.ToString();
                                if (!added.Contains(segment) && !IsInvalidSegment(segment))
                                {
                                    if (editedWord)
                                    {
                                        wordsIndex.Add((i, word));
                                    }
                                    added.Add(segment);
                                    store.CountOne(segment);
                                    if (maxMatchWords < matchWords)
                                    {
                                        maxMatchWords = matchWords;
                                    }
                                }
                            }
                        }
                    }

                    editedWord = true;
                }
                while (word.Length - 1 >= MinLettersForWord && (!UseWordPrefixes || WordPrefixes.Any(x => word.StartsWith(x))));

                i += maxMatchWords - 1;
            }

            var sentenceIndex = store.Insert(words);
            foreach (var item in wordsIndex)
            {
                store.Insert(sentenceIndex, item.Index, item.Word);
            }
        }
    }
    private string[] GetWords(string sentence)
    {
        var shortens = store.GetShortenWords();
        return SplitWords(sentence)
            .Where(x => x != null && x.Length >= MinLettersForWord)
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

    public void CreateGroup(string key, IEnumerable<string> keys)
    {
        store.CreateGroup(key, keys);
    }
    public void CreateShortenWord(string shorten, string key)
    {
        store.CreateShortenWord(shorten, key);
    }

    public void Clear() =>
        store.Clear();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        store.Dispose();
    }

}
