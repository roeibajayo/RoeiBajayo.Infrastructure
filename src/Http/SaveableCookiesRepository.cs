using RoeiBajayo.Infrastructure.Repositories.Files;
using System.Collections.Generic;

namespace RoeiBajayo.Infrastructure.Http;

public class SaveableCookiesRepository : InMemoryCookiesRepository
{
    public SaveableCookiesRepository(string name)
    {
        storage = new("cookies_" + name + ".json");

        if (storage.TryLoad(out var cookies))
            Cookies = cookies!;
    }

    private readonly FileStorage<Dictionary<string, Dictionary<string, string>>> storage;

    public void Save() =>
        storage.Save(Cookies);

    public override void Add(string host, string key, string value)
    {
        base.Add(host, key, value);
        Save();
    }

    public override void Remove(string host)
    {
        base.Remove(host);
        Save();
    }

    public override void Remove(string host, string key)
    {
        base.Remove(host, key);
        Save();
    }

    public override void Clear(string host)
    {
        base.Clear(host);
        Save();
    }

    public override void Clear()
    {
        base.Clear();
        storage.Clear();
    }
}
