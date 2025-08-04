using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Utils.Repositories.Persistent;

public class JsonPersistentCollection<T> : PersistentCollection<T> where T : class
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        IgnoreReadOnlyProperties = true
    };

    public JsonPersistentCollection(bool useIndexFile = true, int intervalMiliseconds = 1000) :
        base(useIndexFile, intervalMiliseconds)
    { }

    public JsonPersistentCollection(string basePath, bool useIndexFile, int intervalMiliseconds = 1000) :
        this(basePath, typeof(T).Name, useIndexFile, intervalMiliseconds)
    { }

    public JsonPersistentCollection(string basePath, string name, bool useIndexFile, int intervalMiliseconds = 1000) :
        base(basePath, name, useIndexFile, intervalMiliseconds)
    { }

    protected override byte[] SerializeDocument<TType>(TType item) =>
        JsonSerializer.SerializeToUtf8Bytes(item, jsonOptions);
    protected override TType DeserializeDocument<TType>(ReadOnlySpan<byte> bytes) =>
        JsonSerializer.Deserialize<TType>(bytes, jsonOptions)!;
    protected override TType DeserializeDocument<TType>(Stream stream) =>
        JsonSerializer.Deserialize<TType>(stream)!;
}
