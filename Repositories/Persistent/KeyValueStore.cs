using Infrastructure.Utils.DependencyInjection.Interfaces;
using Infrastructure.Utils.Repositories.Files;
using System.Collections.Generic;
using System.Text.Json;

namespace Infrastructure.Utils.Repositories.Persistent;

public interface IKeyValueStore
{
    T? Get<T>(string key);
    IEnumerable<string> Keys();
    void Set<T>(string key, T value);
    bool Remove(string key);
    bool Contains(string key);
    void Clear();
    void SaveChanges();
}

internal class KeyValueStore : IKeyValueStore, ISingletonService<IKeyValueStore>
{
    private const string FILENAME = "KeyValueStore.json";

    private readonly IDictionary<string, object> _keyValueStore;
    private readonly FileStorage<IDictionary<string, object>> _storage = new(FILENAME);

    public KeyValueStore()
    {
        _storage.TryLoad(out var keyValueStore);
        _keyValueStore = keyValueStore ?? new Dictionary<string, object>();
    }

    public T? Get<T>(string key)
    {
        return _keyValueStore.TryGetValue(key, out var value) ?
            JsonSerializer.Deserialize<T>(((JsonElement)value).GetRawText()) :
            default;
    }

    public IEnumerable<string> Keys()
    {
        return _keyValueStore.Keys;
    }

    public void Set<T>(string key, T value)
    {
        if (value is not null)
            _keyValueStore[key] = value;
    }

    public bool Remove(string key)
    {
        return _keyValueStore.Remove(key);
    }

    public bool Contains(string key)
    {
        return _keyValueStore.ContainsKey(key);
    }

    public void Clear()
    {
        _keyValueStore.Clear();
    }

    public void SaveChanges()
    {
        if (_keyValueStore.Count == 0)
            _storage.Clear();
        else
            _storage.Save(_keyValueStore);
    }
}
