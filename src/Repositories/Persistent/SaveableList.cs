using RoeiBajayo.Infrastructure.Repositories.Files;
using System.Collections.Generic;

namespace RoeiBajayo.Infrastructure.Repositories.Persistent;

public class SaveableList<T> : List<T>
{
    private readonly string? filename = null;
    private readonly string? path = null;
    private readonly FileStorage<List<T>> storage = new();

    public SaveableList()
    {
        filename = nameof(T) + "s.json";

        Load();
    }
    public SaveableList(string filename)
    {
        this.filename = filename;

        Load();
    }
    public SaveableList(string filename, string path)
    {
        this.filename = filename;
        this.path = path;

        Load();
    }

    private void Load()
    {
        storage.TryLoad(filename!, path, out var items);
        AddRange(items ?? []);
    }

    public void Save()
    {
        storage.Save(this, filename!, path);
    }

    public new void Clear()
    {
        storage.Clear(filename!, path);
        base.Clear();
    }
}
