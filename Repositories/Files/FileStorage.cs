using Infrastructure.Utils.Repositories.Files.Serializers;
using System;
using System.IO;
using System.Threading;

namespace Infrastructure.Utils.Repositories.Files;

public class FileStorage<T>(ISerializer<T> serializer)
{
    private const string DEFAULT_FILENAME = "storage.data";
    private readonly ISerializer<T> serializer = serializer;
#if NET9_0_OR_GREATER
    private readonly Lock locker = new();
#else
    private readonly object locker = new();
#endif

    public string? Filename { get; }

    public FileStorage(string? filename = null) : this(new JsonSerializer<T>())
    {
        Filename = filename;
    }

    public void Save(T content, string? filename = null, string? path = null)
    {
        lock (locker)
        {
            var json = serializer.Serialize(content);
            var filepath = Path.Combine(path ?? Environment.CurrentDirectory, filename ?? Filename ?? DEFAULT_FILENAME);
            Directory.CreateDirectory(Path.GetDirectoryName(filepath)!);
            File.WriteAllText(filepath, json);
        }
    }

    public bool TryLoad(out T? result) =>
        TryLoad(null, out result);
    public bool TryLoad(string? filename, out T? result) =>
        TryLoad(filename, null, out result);
    public bool TryLoad(string? filename, string? path, out T? result)
    {
        lock (locker)
        {
            var filepath = Path.Combine(path ?? Environment.CurrentDirectory, filename ?? Filename ?? DEFAULT_FILENAME);
            if (File.Exists(filepath))
            {
                var content = File.ReadAllText(filepath);

                if (string.IsNullOrWhiteSpace(content))
                {
                    result = default;
                    return false;
                }

                result = serializer.Deserialize(content);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
    }
    public void Clear(string? filename = null, string? path = null)
    {
        lock (locker)
        {
            var filepath = Path.Combine(path ?? Environment.CurrentDirectory, filename ?? Filename ?? DEFAULT_FILENAME);

            if (File.Exists(filepath))
                File.Delete(filepath);
        }
    }
}
