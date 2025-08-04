using RoeiBajayo.Infrastructure.Expressions;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoeiBajayo.Infrastructure.Repositories.Database;

/// <summary>
/// Infile DB with index file, the items are not saved in memory
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class InfileDatabase<T> : ICollection<T>, IDisposable
{
    private const int BUFFER_SIZE = 4096;

    public readonly string Name;
    private readonly string? _idPropertyName;
    public readonly string BasePath;

    protected int _currentStoragePosition;

    protected Dictionary<int, Tuple<uint, ushort>> Index = [];
    protected readonly string _storageFilePath;

    protected readonly string _indexFilePath;
    protected readonly PropertyInvoker<T, int>? _idProperty;
    protected int _currentDocumentIndex;

    protected readonly object FILE_LOCKER = new();
    protected readonly object ACTION_LOCKER = new();

    public InfileDatabase(string basePath, string name, string? idPropertyName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(null, nameof(name));

        if (string.IsNullOrWhiteSpace(basePath))
            throw new ArgumentException(null, nameof(basePath));

        //path spell validation
        Directory.CreateDirectory(basePath);

        Name = name;
        _idPropertyName = idPropertyName;
        BasePath = basePath;

        _storageFilePath = Path.Combine(BasePath, Name + ".db");
        _indexFilePath = Path.Combine(BasePath, Name + "_index.db");

        try
        {
            _idProperty = new PropertyInvoker<T, int>(idPropertyName ?? "Id");
        }
        catch { }

        LoadIndex();
    }
    ~InfileDatabase()
    {
        Dispose();
    }

    protected void LoadIndex()
    {
        if (_idProperty == null)
            return;

        lock (FILE_LOCKER)
        {
            if (!File.Exists(_indexFilePath) || !File.Exists(_storageFilePath))
            {
                Clear();
                return;
            }

            var index = new Dictionary<int, Tuple<uint, ushort>>();
            byte[] buffer = ArrayPool<byte>.Shared.Rent(10);
            using var content = new BufferedStream(File.OpenRead(_indexFilePath), BUFFER_SIZE);
            try
            {
                var read = 0;
                byte action = buffer[0]; //0=insert,1=deleted
                ushort nextLength = 0;
                var id = 0;
                uint position = 0;

                while ((read = content.Read(buffer, 0, 1)) != 0)
                {
                    if (action == 0)
                    {
                        //insert
                        content.ReadExactly(buffer, 0, 10);
                        id = BitConverter.ToInt32(new ReadOnlySpan<byte>(buffer, 0, 4));
                        if (_currentDocumentIndex < id)
                        {
                            _currentDocumentIndex = id;
                        }
                        position = BitConverter.ToUInt32(new ReadOnlySpan<byte>(buffer, 4, 4));
                        nextLength = BitConverter.ToUInt16(new ReadOnlySpan<byte>(buffer, 8, 2));
                        _currentStoragePosition += nextLength;

                        var indexItem = new Tuple<uint, ushort>(position, nextLength);
                        if (!index.TryAdd(id, indexItem))
                        {
                            index[id] = indexItem;
                        }
                    }
                    else
                    {
                        //delete
                        content.ReadExactly(buffer, 0, 4);
                        id = BitConverter.ToInt32(new ReadOnlySpan<byte>(buffer, 0, 4));
                        index.Remove(id);
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            Index = index;
        }
    }


    public void Add(T item)
    {
        AddRange([item]);
    }
    public void AddRange(IEnumerable<T> items)
    {
        lock (ACTION_LOCKER)
        {
            if (_idProperty != null)
            {
                foreach (var item in items)
                {
                    var id = _idProperty.Get(item);

                    if (id == 0)
                    {
                        id = ++_currentDocumentIndex;
                    }
                    else
                    {
                        if (Index.ContainsKey(id))
                        {
                            throw new InvalidDataException($"Id {id} already exists");
                        }
                        else
                        {
                            if (id >= _currentDocumentIndex)
                            {
                                _currentDocumentIndex = id;
                            }
                        }
                    }

                    _idProperty.Set(item, id);
                }
            }

            lock (FILE_LOCKER)
            {
                unchecked
                {
                    using var storageFile = new BufferedStream(File.Open(_storageFilePath, FileMode.Append), BUFFER_SIZE);
                    using var tempIndexFile = new MemoryStream(!items.TryCount(out var count) ? 0 : 11 * count);

                    foreach (var item in items)
                    {
                        var documentBytes = SerializeDocument(item);
                        storageFile.Write(documentBytes);

                        var id = _idProperty?.Get(item) ?? ++_currentDocumentIndex;
                        tempIndexFile.WriteByte(0); //insert
                        tempIndexFile.WriteByte((byte)id); //id
                        tempIndexFile.WriteByte((byte)(id >> 8));
                        tempIndexFile.WriteByte((byte)(id >> 16));
                        tempIndexFile.WriteByte((byte)(id >> 24));
                        tempIndexFile.WriteByte((byte)_currentStoragePosition); //position
                        tempIndexFile.WriteByte((byte)(_currentStoragePosition >> 8));
                        tempIndexFile.WriteByte((byte)(_currentStoragePosition >> 16));
                        tempIndexFile.WriteByte((byte)(_currentStoragePosition >> 24));
                        tempIndexFile.WriteByte((byte)documentBytes.Length); //length
                        tempIndexFile.WriteByte((byte)(documentBytes.Length >> 8));
                        tempIndexFile.WriteByte((byte)(documentBytes.Length >> 16));
                        tempIndexFile.WriteByte((byte)(documentBytes.Length >> 24));

                        var indexItem = new Tuple<uint, ushort>((uint)_currentStoragePosition, (ushort)documentBytes.Length);
                        if (!Index.TryAdd(id, indexItem))
                        {
                            Index[id] = indexItem;
                        }

                        _currentStoragePosition += documentBytes.Length;
                    }

                    storageFile.Flush();

                    using var indexFile = File.Open(_indexFilePath, FileMode.Append);
                    tempIndexFile.CopyTo(indexFile);
                }
            }
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
        foreach (var item in items)
        {
            Replace(item);
        }
    }

    protected void IdPropertyRequired()
    {
        if (_idProperty is null)
            InfileDatabase<T>.NoIdPropertyException();
    }
    private static void NoIdPropertyException() =>
        throw new InvalidOperationException("Cannot remove item without Id property");

    public bool Remove(T item)
    {
        return Remove([item]);
    }
    public bool Remove(IEnumerable<T> items)
    {
        IdPropertyRequired();

        lock (ACTION_LOCKER)
        {
            lock (FILE_LOCKER)
            {
                unchecked
                {
                    using var indexFile = new BufferedStream(File.Open(_indexFilePath, FileMode.Append), BUFFER_SIZE);
                    foreach (var item in items)
                    {
                        var id = _idProperty!.Get(item);
                        indexFile.WriteByte(1); //delete
                        indexFile.WriteByte((byte)id); //id
                        indexFile.WriteByte((byte)(id >> 8));
                        indexFile.WriteByte((byte)(id >> 16));
                        indexFile.WriteByte((byte)(id >> 24));
                        Index.Remove(id);
                    }
                }
            }
        }
        return true;
    }

    public void Optimize()
    {
        lock (ACTION_LOCKER)
        {
            var tempName = Name + "_temp";
            var tempStorageFilePath = Path.Combine(BasePath, tempName + ".db");
            var tempIndexFilePath = Path.Combine(BasePath, tempName + "_index.db");

            using (var tempStorage = CreateInstance(BasePath, tempName))
            {
                tempStorage.AddRange(this);
                Clear();
                _currentDocumentIndex = tempStorage._currentDocumentIndex;
                _currentStoragePosition = tempStorage._currentStoragePosition;
                Index = tempStorage.Index;
            }

            File.Delete(_storageFilePath);
            File.Move(tempStorageFilePath, _storageFilePath);

            File.Delete(_indexFilePath);
            File.Move(tempIndexFilePath, _indexFilePath);
        }
    }

    public void Clear()
    {
        File.Delete(_storageFilePath);
        File.Delete(_indexFilePath);
        _currentDocumentIndex = 0;
        _currentStoragePosition = 0;
        Index.Clear();
    }

    public int Count =>
        Index.Count;

    public bool IsReadOnly =>
        false;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    protected abstract byte[] SerializeDocument<TType>(TType item);
    protected abstract TType DeserializeDocument<TType>(ReadOnlySpan<byte> bytes);
    protected InfileDatabase<T> CreateInstance(string basePath, string name) =>
        (InfileDatabase<T>)Activator.CreateInstance(GetType(), [basePath, name, _idPropertyName])!;

    public bool Contains(T item) =>
        Contains(_idProperty?.Get(item) ?? 0);
    public bool Contains(int id)
    {
        IdPropertyRequired();
        return Index.ContainsKey(id);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new InfileDatabaseEnumerator(this);
    }

    IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    protected class InfileDatabaseEnumerator : IEnumerator<T>
    {
        private readonly InfileDatabase<T> _db;
        private readonly IEnumerator<Tuple<uint, ushort>>? _indexEnumerator;

        private readonly BufferedStream? _content;
        private int _position;

        public InfileDatabaseEnumerator(InfileDatabase<T> db)
        {
            _db = db;

            if (_db.Count != 0 && File.Exists(_db._storageFilePath))
            {
                _content = new BufferedStream(File.OpenRead(_db._storageFilePath), BUFFER_SIZE);
                _indexEnumerator = _db.Index.Values.OrderBy(x => x.Item1).GetEnumerator();
            }
        }

        public T Current { get; private set; } = default!;
        object IEnumerator.Current => Current!;

        public void Dispose()
        {
            _content?.Dispose();
        }

        public void Reset()
        {
            Current = default!;
        }

        public bool MoveNext()
        {
            if (!_indexEnumerator!.MoveNext())
                return false;

            var indexItem = _indexEnumerator.Current;

            if (_position != indexItem.Item1)
            {
                _position = (int)indexItem.Item1;
                _content!.Seek(_position, SeekOrigin.Begin);
            }

            var _buffer = ArrayPool<byte>.Shared.Rent(indexItem.Item2);
            var length = _content!.Read(_buffer, 0, indexItem.Item2);
            Current = _db.DeserializeDocument<T>(new ReadOnlySpan<byte>(_buffer, 0, length));
            ArrayPool<byte>.Shared.Return(_buffer);

            _position += indexItem.Item2;

            return true;
        }
    }
}
