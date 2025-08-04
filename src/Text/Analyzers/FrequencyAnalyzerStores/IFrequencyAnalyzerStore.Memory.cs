using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Utils.Text.Analyzers.FrequencyAnalyzerStores;

public class MemoryStore : IFrequencyAnalyzerStore
{
    private readonly ConcurrentDictionary<string, MemoryStoreItem> _items = new();
    private readonly Dictionary<string, string> _groups = [];
    private readonly Dictionary<string, string> _shortenWords = [];

    public bool TryCountOne(string key)
    {
        if (_items.TryAdd(key, new MemoryStoreItem { Counter = 1 }))
            return true;

        _items[key].Counter++;
        return false;
    }
    public bool Insert(string key, int value)
    {
        if (_items.TryAdd(key, new MemoryStoreItem { Counter = value }))
            return true;

        _items[key].Counter += value;
        return false;
    }
    public void CountIfExists(string key)
    {
        if (_items.TryGetValue(key, out var item))
        {
            item.Counter++;
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
            .Where(x => x.Value.Counter >= minimumCounter)
            .ToDictionary(x => x.Key, x => x.Value.Counter);
    }
    public bool Exists(string key)
    {
        return _items.ContainsKey(key);
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

    public void Replace(Dictionary<string, int> with)
    {
        _items.Clear();
        foreach (var item in with)
            _items.TryAdd(item.Key, new MemoryStoreItem { Counter = item.Value });
    }

    protected class MemoryStoreItem
    {
        public int Counter = 0;
    }
}