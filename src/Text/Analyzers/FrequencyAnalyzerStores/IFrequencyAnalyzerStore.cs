using System;
using System.Collections.Generic;

namespace RoeiBajayo.Infrastructure.Text.Analyzers.FrequencyAnalyzerStores;

public interface IFrequencyAnalyzerStore : IDisposable
{
    bool TryCountOne(string key);
    bool Insert(string key, int value);
    void Replace(Dictionary<string, int> with);

    void CreateGroup(string key, IEnumerable<string> keys);
    IReadOnlyDictionary<string, string> GetGroups();

    void CreateShortenWord(string shorten, string key);
    IReadOnlyDictionary<string, string> GetShortenWords();

    bool Exists(string key);
    void Clear();
    void CountIfExists(string key);
    IReadOnlyDictionary<string, int> Get(int minimumCounter = 3);
}
