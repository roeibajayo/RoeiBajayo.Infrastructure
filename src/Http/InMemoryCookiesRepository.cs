using RoeiBajayo.Infrastructure.Http.Models;
using System.Collections.Generic;
using System.Linq;

namespace RoeiBajayo.Infrastructure.Http;

public class InMemoryCookiesRepository : ICookiesRepository
{
    public Dictionary<string, Dictionary<string, string>> Cookies { get; init; } = [];

    public IDictionary<string, string> GetAll() =>
        Cookies
            .SelectMany(x => x.Value)
            .ToDictionary(x => x.Key, x => x.Value);

    public IDictionary<string, string> Get(string host)
    {
        if (!Cookies.TryGetValue(host, out Dictionary<string, string>? value))
            return new Dictionary<string, string>();

        return value;
    }
    public virtual void Add(string host, string key, string value)
    {
        if (!Cookies.TryGetValue(host, out Dictionary<string, string>? hostsCookies))
        {
            Cookies[host] = new() { { key, value } };
            return;
        }

        hostsCookies[key] = value;
    }

    public virtual void Remove(string host)
    {
        Cookies.Remove(host);
    }
    public virtual void Remove(string host, string key)
    {
        if (!Cookies.TryGetValue(host, out Dictionary<string, string>? value))
            return;

        value.Remove(key);
    }
    public virtual void Clear(string host)
    {
        Cookies.Remove(host);
    }
    public virtual void Clear()
    {
        Cookies.Clear();
    }
}
