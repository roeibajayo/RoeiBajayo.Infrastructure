using System.IO;

namespace RoeiBajayo.Infrastructure.Http.Models;

public class MultipartFormDataValue
{
    public MultipartFormDataValue(string filename, Stream stream, string contentType = "application/data")
    {
        Filename = filename;
        Stream = stream;
        ContentType = contentType;
    }
    public MultipartFormDataValue(object value)
    {
        Value = value;
    }

    public object? Value { get; }
    public string? Filename { get; }
    public Stream? Stream { get; }
    public string? ContentType { get; }

    public bool IsFile =>
        Filename != null && Stream != null;
}
