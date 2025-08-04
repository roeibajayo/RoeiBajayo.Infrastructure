using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;

namespace RoeiBajayo.Infrastructure.Repositories.Persistent;

public class PersistentAppendList<T>
{
#if NET9_0_OR_GREATER
    private readonly Lock locker = new();
#else
    private readonly object locker = new();
#endif
    private readonly string filename;
    private readonly string? path;

    public PersistentAppendList()
    {
        filename = nameof(T) + "s.json";
    }
    public PersistentAppendList(string filename)
    {
        this.filename = filename;
    }
    public PersistentAppendList(string filename, string path)
    {
        this.filename = filename;
        this.path = path;
    }

    public void Append(T content) =>
        Append([content]);
    public void Append(IEnumerable<T> contents)
    {
        lock (locker)
        {
            var filepath = Path.Combine(path ?? Environment.CurrentDirectory, filename);
            Directory.CreateDirectory(Path.GetDirectoryName(filepath)!);
            using var file = File.Open(filepath, FileMode.Append);
            using var writer = new StreamWriter(file);
            foreach (var content in contents)
            {
                writer.WriteLine(JsonSerializer.Serialize(content, jsonSerializerOptions));
                writer.Flush();
                file.Flush();
            }
        }
    }

    public IEnumerable<T> GetAll()
    {
        lock (locker)
        {
            var filepath = Path.Combine(path ?? Environment.CurrentDirectory, filename);

            if (!File.Exists(filepath))
                return [];

            var lines = File.ReadAllLines(filepath);
            if (lines.Length == 0)
                return [];

            return DeserializeLines(lines);
        }
    }

    private static IEnumerable<T> DeserializeLines(string[] lines)
    {
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            yield return JsonSerializer.Deserialize<T>(line, jsonSerializerOptions)!;
        }
    }

    public void Clear()
    {
        lock (locker)
        {
            var filepath = Path.Combine(path ?? Environment.CurrentDirectory, filename);

            if (File.Exists(filepath))
                File.Delete(filepath);
        }
    }

    private static JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = false,
        IncludeFields = false,
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}
