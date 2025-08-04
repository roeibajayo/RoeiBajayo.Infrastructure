using System.Text.Encodings.Web;
using System.Text.Json;

namespace Infrastructure.Utils.Repositories.Files.Serializers;

public class JsonSerializer<T> : ISerializer<T>
{
    private readonly JsonSerializerOptions jsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = false,
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = true,
        
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public string? Serialize(T? content) =>
        content is null ? null : JsonSerializer.Serialize(content, jsonOptions);

    public T? Deserialize(string? content) =>
        string.IsNullOrEmpty(content) ? default : JsonSerializer.Deserialize<T>(content, jsonOptions);
}

public static class JsonSerializerExtensions
{
    public static string? ToJson<T>(this T content) =>
        new JsonSerializer<T>().Serialize(content);
}
