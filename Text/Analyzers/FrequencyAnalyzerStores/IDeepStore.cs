using System.Collections.Generic;

namespace Infrastructure.Utils.Text.Analyzers.FrequencyAnalyzerStores;

public interface IDeepStore
{
    void Clear();
    void CountOne(string segment);
    void CreateGroup(string groupKey, IEnumerable<string> keys);
    void CreateShortenWord(string shorten, string key);
    void Dispose();
    IReadOnlyDictionary<string, int> Get(int minimumCounter = 3);
    IReadOnlyDictionary<string, string> GetGroups();
    IEnumerable<(string Sentence, int WordIndex)> GetSentences(string startWord);
    IReadOnlyDictionary<string, string> GetShortenWords();
    void Insert(int sentenceIndex, int wordIndex, string word);
    int Insert(string[] words);
}