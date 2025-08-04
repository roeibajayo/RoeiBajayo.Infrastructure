using Infrastructure.Utils.Expressions;
using Infrastructure.Utils.Repositories.Queues;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Infrastructure.Utils.Repositories.Persistent;

/// <summary>
/// Infile collection with option to use index file so all changes will append to the file instead of rewrite whole file.
/// the items are saved in memory
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PersistentCollection<T> : ICollection<T>, IDisposable where T : class
{
    public readonly string Name;
    public readonly string BasePath;

    protected List<Tuple<uint, T>> _storage = [];
    protected uint _currentDocumentIndex;
    protected int _currentIndex;

    protected PropertyInvoker<T, int> _idProperty;

    protected readonly bool _useIndexFile;
    protected readonly string _storageFilePath;

    protected readonly AccumulatorQueue<ActionLog> _accumulatorBulkProcess;

    protected readonly object FILE_LOCKER = new();
    protected readonly object ACTION_LOCKER = new();

    public PersistentCollection(bool useIndexFile = true, int intervalMiliseconds = 1000) :
        this(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.FullName)!, typeof(T).Name, useIndexFile, intervalMiliseconds)
    {

    }
    public PersistentCollection(string basePath, bool useIndexFile = true, int intervalMiliseconds = 1000) :
        this(basePath, typeof(T).Name, useIndexFile, intervalMiliseconds)
    {
    }
    public PersistentCollection(string basePath, string name, bool useIndexFile = true, int intervalMiliseconds = 1000)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(null, nameof(name));

        if (string.IsNullOrWhiteSpace(basePath))
            throw new ArgumentException(null, nameof(basePath));

        //path spell validation
        Directory.CreateDirectory(basePath);

        Name = name;
        BasePath = basePath;

        _useIndexFile = useIndexFile;
        _storageFilePath = Path.Combine(BasePath, Name + (useIndexFile ? ".collection" : ".json"));

        _idProperty = new PropertyInvoker<T, int>("Id");
        _accumulatorBulkProcess =
            new AccumulatorQueue<ActionLog>(ProcessActions, intervalMiliseconds);

        Load();
    }
    ~PersistentCollection()
    {
        Dispose();
    }

    protected virtual void Load()
    {
        lock (FILE_LOCKER)
        {
            if (!File.Exists(_storageFilePath))
            {
                Clear();
                return;
            }

            if (_useIndexFile)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(5000);
                using var content = new BufferedStream(File.OpenRead(_storageFilePath));
                try
                {
                    var read = 0;
                    if ((read = content.Read(buffer, 0, 2)) != 0)
                    {
                        uint i = 0;
                        var nextLength = 0;
                        var skipIndex = 0;

                        while (true)
                        {
                            _currentDocumentIndex++;

                            nextLength = BitConverter.ToUInt16(new ReadOnlySpan<byte>(buffer, read - 2, 2));

                            if (nextLength != 0)
                            {
                                _currentIndex++;
                                read = content.Read(buffer, 0, nextLength + 2);

                                _storage.Add(new Tuple<uint, T>(i++,
                                    DeserializeDocument<T>(new ReadOnlySpan<byte>(buffer, 0, nextLength))));

                                if (read == nextLength)
                                    break;
                            }
                            else
                            {
                                read = content.Read(buffer, 0, 6);

                                skipIndex = BitConverter.ToInt32(new ReadOnlySpan<byte>(buffer, 0, 4));
                                _storage.TryRemoveWhere(x => x.Item1 == skipIndex);

                                if (read == 2)
                                    break;
                            }
                        }
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            else
            {
                using var fileStream = File.OpenRead(_storageFilePath);
                var values = DeserializeDocument<T[]>(fileStream);
                _storage.Capacity = values.Length;

                for (uint i = 0; i < values.Length; i++)
                {
                    _storage.Add(new Tuple<uint, T>(i, values[i]));
                }
            }
        }
    }


    public int Interval
    {
        get => _accumulatorBulkProcess.Interval;
        set => _accumulatorBulkProcess.Interval = value;
    }

    public T? Find(int id)
    {
        lock (ACTION_LOCKER)
        {
            if (_idProperty != null)
            {
                return this.FirstOrDefault(x => _idProperty.Get(x) == id);
            }

            return default;
        }
    }
    public void Add(T item)
    {
        lock (ACTION_LOCKER)
        {
            _idProperty?.Set(item, ++_currentIndex);

            _storage.Add(new Tuple<uint, T>(_currentDocumentIndex++, item));
            _accumulatorBulkProcess.Enqueue(new ActionLog
            {
                Action = ActionLog.Actions.Insert,
                Item = item
            });
        }
    }
    public int AddRange(IEnumerable<T> items)
    {
        lock (ACTION_LOCKER)
        {
            if (_idProperty != null)
            {
                foreach (var item in items)
                    _idProperty.Set(item, ++_currentIndex);
            }

            items.TryCount(out var count);
            if (count > 0)
            {
                _storage.Capacity = _storage.Count + count;
            }

            count = 0;
            foreach (var item in items)
            {
                _storage.Add(new Tuple<uint, T>(_currentDocumentIndex++, item));
                count++;
            }

            _accumulatorBulkProcess.Enqueue(items.Select(item => new ActionLog
            {
                Action = ActionLog.Actions.Insert,
                Item = item
            }).ToArray());

            return count;
        }
    }

    public void Update(T item) =>
        Replace(item);
    public void Update(IEnumerable<T> items) =>
        Replace(items);

    public void Replace(T item)
    {
        Remove(item);
        Add(item);
    }
    public void Replace(IEnumerable<T> items)
    {
        foreach (var item in items.ToArray())
        {
            Replace(item);
        }
    }

    public bool Remove(T item)
    {
        lock (ACTION_LOCKER)
        {
            for (var i = 0; i < _storage.Count; i++)
            {
                if (_storage[i].Item2.Equals(item))
                {
                    RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
    }
    public int Remove(IEnumerable<T> items)
    {
        lock (ACTION_LOCKER)
        {
            return items.Count(item => Remove(item));
        }
    }
    public bool RemoveAt(int index)
    {
        lock (ACTION_LOCKER)
        {
            var item = _storage[index];
            _storage.RemoveAt(index);
            _currentDocumentIndex++;
            _accumulatorBulkProcess.Enqueue(new ActionLog
            {
                Action = ActionLog.Actions.Delete,
                Index = item.Item1
            });
            return true;
        }
    }

    public void Optimaze()
    {
        if (!_useIndexFile)
            return;

        lock (ACTION_LOCKER)
        {
            ForceSave();

            var tempName = Name + "_temp";
            var tempStorageFilePath = Path.Combine(BasePath, tempName + ".collection");

            var tempStorage = CreateInstance(BasePath, tempName);
            tempStorage.AddRange(_storage.Select(x => x.Item2));

            CopyFrom(tempStorage);

            tempStorage.Dispose();

            File.Delete(_storageFilePath);
            File.Move(tempStorageFilePath, _storageFilePath);
        }
    }

    public void CopyFrom(PersistentCollection<T> collection)
    {
        _storage = [.. collection._storage];
        _currentDocumentIndex = collection._currentDocumentIndex;
    }

    public void ForceSave() =>
        _accumulatorBulkProcess.ForceExecute();
    private bool _ignoreNextProcess = false;
    protected virtual void ProcessActions(IEnumerable<ActionLog> items)
    {
        if (_ignoreNextProcess)
        {
            _ignoreNextProcess = false;
            return;
        }

        lock (FILE_LOCKER)
        {
            if (_useIndexFile)
            {
                Stream storageFile = new BufferedStream(File.Open(_storageFilePath, FileMode.Append));

                foreach (var item in items)
                {
                    if (item.Action == ActionLog.Actions.Delete)
                    {
                        unchecked
                        {
                            storageFile.WriteByte(0);
                            storageFile.WriteByte(0);
                            storageFile.WriteByte((byte)item.Index);
                            storageFile.WriteByte((byte)(item.Index >> 8));
                            storageFile.WriteByte((byte)(item.Index >> 16));
                            storageFile.WriteByte((byte)(item.Index >> 24));
                        }
                    }
                    else
                    {
                        var documentBytes = SerializeDocument(item.Item);
                        unchecked
                        {
                            storageFile.WriteByte((byte)documentBytes.Length);
                            storageFile.WriteByte((byte)(documentBytes.Length >> 8));
                        }
                        storageFile.Write(documentBytes);
                    }
                }

                storageFile?.Flush();
                storageFile?.Dispose();
            }
            else
            {
                var tempStorageFilePath = Path.Combine(BasePath, Name + "_saving.json");
                File.WriteAllBytes(tempStorageFilePath, SerializeDocument(_storage.Select(x => x.Item2)));
                File.Delete(_storageFilePath);
                File.Move(tempStorageFilePath, _storageFilePath);
            }
        }
    }

    public void Clear()
    {
        File.Delete(_storageFilePath);
        _currentDocumentIndex = 0;
        _storage.Clear();
    }

    public int Count => _storage.Count;

    public bool IsReadOnly => false;

    public T this[int index]
    {
        get => _storage[index].Item2;
    }

    public IEnumerator<T> GetEnumerator() =>
        _storage.Select(x => x.Item2).GetEnumerator();
    IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        _storage.GetEnumerator();

    public bool Contains(T item) =>
        _storage.Select(x => x.Item2).Contains(item);
    public void CopyTo(T[] array, int arrayIndex) =>
        _storage.Select(x => x.Item2).ToArray().CopyTo(array, arrayIndex);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        ForceSave();
        _accumulatorBulkProcess.Dispose();
        _storage.Clear();
    }

    protected class ActionLog
    {
        public enum Actions { Insert, Delete };
        public T? Item;
        public uint Index;
        public Actions Action;
    }

    protected abstract byte[] SerializeDocument<TType>(TType item);
    protected abstract TType DeserializeDocument<TType>(ReadOnlySpan<byte> bytes);
    protected abstract TType DeserializeDocument<TType>(Stream stream);
    protected PersistentCollection<T> CreateInstance(string basePath, string name) =>
        (PersistentCollection<T>)Activator.CreateInstance(GetType(),
            [basePath, name, _useIndexFile, Interval])!;
}
