using System;
using System.Text.Json;

namespace RoeiBajayo.Infrastructure.Repositories.Database;

public class JsonInfileDatabase<T>(string basePath, string name, string? idProperty = null) : 
    InfileDatabase<T>(basePath, name, idProperty)
{
    protected override TType DeserializeDocument<TType>(ReadOnlySpan<byte> bytes)
    {
        return JsonSerializer.Deserialize<TType>(bytes)!;
    }

    protected override byte[] SerializeDocument<TType>(TType item)
    {
        return JsonSerializer.SerializeToUtf8Bytes(item);
    }
}
