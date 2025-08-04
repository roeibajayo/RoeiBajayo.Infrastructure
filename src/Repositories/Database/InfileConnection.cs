using MemoryCore;
using System;
using System.IO;
using System.Reflection;

namespace Infrastructure.Utils.Repositories.Database;

public abstract class InfileConnection<T>(string basePath, string name) : IDisposable
{
    protected static readonly MemoryCoreManager _cache = new();

    protected readonly string _basePath = basePath;
    protected readonly string _name = name;
    protected readonly int _intervalMiliseconds;

    protected readonly string _cacheKey = basePath + "_" + name;

    public InfileConnection() :
        this(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.FullName)!)
    { }

    public InfileConnection(string basePath) :
        this(basePath, typeof(T).Name)
    { }

    public InfileDatabase<T> Collection =>
        _cache.TryGetOrAddSliding(_cacheKey,
            () => CreateCollectionInstance(),
            duration: TimeSpan.FromMinutes(10))!;

    public void Kill() =>
        _cache.Remove(_cacheKey);

    public void Dispose() { GC.SuppressFinalize(this); }

    protected abstract InfileDatabase<T> CreateCollectionInstance();
}
