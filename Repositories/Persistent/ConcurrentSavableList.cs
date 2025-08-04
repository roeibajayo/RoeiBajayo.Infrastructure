using Infrastructure.Utils.Repositories.Files;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Utils.Repositories.Persistent;

public class ConcurrentSavableList<T> : ConcurrentBag<T>, ICollection<T>
{
    private readonly string? filename = null;
    private readonly string? path = null;
    private readonly FileStorage<IEnumerable<T>> storage = new();

    public bool AutoSave { get; set; } = true;
    public bool IsReadOnly => false;

    public ConcurrentSavableList()
    {
        filename = typeof(T).Name + "s.json";

        Load();
    }
    public ConcurrentSavableList(string filename)
    {
        this.filename = filename;

        Load();
    }
    public ConcurrentSavableList(string filename, string path)
    {
        this.filename = filename;
        this.path = path;

        Load();
    }

    private void Load()
    {
        storage.TryLoad(filename!, path, out var items);

        if (items is null)
            return;

        foreach (var item in items)
        {
            base.Add(item);
        }
    }

    public void Save()
    {
        storage.Save(this, filename!, path);
    }

    public new void Clear()
    {
        if (Count == 0)
            return;

        storage.Clear(filename!, path);
        base.Clear();

        if (AutoSave)
            Save();
    }

    public bool Contains(T item)
    {
        return (this as IEnumerable<T>).Contains(item);
    }

    public bool Remove(T item)
    {
        lock (this)
        {
            var found = false;
            var list = ToArray();

            base.Clear();
            foreach (var x in list)
            {
                if (x?.Equals(item) ?? false)
                {
                    found = true;
                }
                else
                {
                    base.Add(x);
                }
            }

            if (found)
                Save();

            return found;
        }
    }

    public new void Add(T item)
    {
        base.Add(item);

        if (AutoSave)
            Save();
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            base.Add(item);
        }

        if (AutoSave)
            Save();
    }
}
