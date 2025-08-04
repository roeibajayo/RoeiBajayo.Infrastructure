using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Utils.Text.Analyzers.FrequencyAnalyzerStores;

public class DeepMemoryStore : IDeepStore
{
    //indexes
    private readonly List<string> _sentences = [];
    private readonly Dictionary<string, List<WordIndexItem>> _words = [];

    //segments
    private readonly Dictionary<string, int> _items = [];

    //utils
    private readonly Dictionary<string, string> _groups = [];
    private readonly Dictionary<string, string> _shortenWords = [];

    public IEnumerable<(string Sentence, int WordIndex)> GetSentences(string startWord)
    {
        if (_words.TryGetValue(startWord, out var list))
        {
            foreach (var item in list.GroupBy(x => (x.SentenceIndex, x.WordIndex)).Select(x => x.First()))
            {
                yield return (_sentences[item.SentenceIndex], item.WordIndex);
            }
        }
    }
    public int Insert(string[] words)
    {
        var sentence = string.Join(' ', words);
        _sentences.Add(sentence);
        var sentenceIndex = _sentences.Count - 1;
        for (var i = 0; i < words.Length; i++)
        {
            var add = new WordIndexItem { SentenceIndex = sentenceIndex, WordIndex = i };
            if (!_words.TryAdd(words[i], [add]))
            {
                _words[words[i]].Add(add);
            }
        }
        return sentenceIndex;
    }
    public void Insert(int sentenceIndex, int wordIndex, string word)
    {
        var add = new WordIndexItem { SentenceIndex = sentenceIndex, WordIndex = wordIndex };
        if (!_words.TryAdd(word, [add]))
        {
            _words[word].Add(add);
        }
    }
    public void CountOne(string segment)
    {
        if (_items.TryGetValue(segment, out var current))
        {
            _items[segment] = current + 1;
        }
        else
        {
            _items.Add(segment, 1);
        }
    }

    public void CreateGroup(string groupKey, IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            _groups.TryAdd(key, groupKey);
        }
    }
    public void CreateShortenWord(string shorten, string key)
    {
        _shortenWords.TryAdd(shorten, key);
    }

    public IReadOnlyDictionary<string, int> Get(int minimumCounter = 3)
    {
        return _items
            .Where(x => x.Value >= minimumCounter)
            .ToDictionary(x => x.Key, x => x.Value);
    }
    public IReadOnlyDictionary<string, string> GetGroups()
    {
        return _groups;
    }
    public IReadOnlyDictionary<string, string> GetShortenWords()
    {
        return _shortenWords;
    }

    public void Clear()
    {
        _items.Clear();
    }
    public void Dispose()
    {
        Clear();
    }

    protected struct WordIndexItem
    {
        public int SentenceIndex;
        public int WordIndex;
    }
}
