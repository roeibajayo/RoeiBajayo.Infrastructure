using System.Collections.Generic;

namespace Infrastructure.Utils.Http.Models;

public interface ICookiesRepository
{
    IDictionary<string, string> GetAll();
    IDictionary<string, string> Get(string host);
    void Add(string host, string key, string value);
    void Remove(string host);
    void Remove(string host, string key);
    void Clear(string host);
    void Clear();
}
