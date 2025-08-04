using RoeiBajayo.Infrastructure.IEnumerable;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO;

public static class StreamExtensions
{
    public static byte[] ReadAllBytes(this Stream stream, int bufferSize = 4096)
    {
        using var result = new MemoryStream();
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) != 0)
                result.Write(buffer, 0, read);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
        return result.ToArray();
    }
    public static async Task<byte[]> ReadAllBytesAsync(this Stream stream)
    {
        var bytes = new byte[stream.Length];
        await stream.ReadExactlyAsync(bytes);
        return bytes;
    }
    public static void WriteAllBytes(this Stream stream, IEnumerable<IList<byte>> bytes, int bufferSize = 4096)
    {
        var builder = new ArrayBuilder<byte>();
        builder.AddRange(bytes);
        builder.WriteTo(stream, bufferSize);
    }
    public static void WriteAllBytes(this Stream stream, byte[] bytes,
        int offset = 0, int bufferSize = 4096)
    {
        foreach (var buffer in bytes.GetSegments(offset, bufferSize))
            stream.Write(buffer);

        stream.Flush();
    }
    public static void WriteAllBytes(this Stream stream, IEnumerable<byte> bytes,
        int offset = 0, int bufferSize = 4096)
    {
        foreach (var buffer in bytes.Chunks(bufferSize, offset))
            stream.Write(buffer);

        stream.Flush();
    }

    public static string ReadAllText(this Stream stream,
        Encoding? encoding = null, int bufferSize = -1, bool leaveOpen = false)
    {
        using var reader = new StreamReader(stream, encoding: encoding ?? Encoding.UTF8,
            bufferSize: bufferSize, leaveOpen: leaveOpen);
        return reader.ReadToEnd();
    }
    public static void WriteAllText(this Stream stream, string text,
        Encoding? encoding = null, int bufferSize = -1, bool leaveOpen = false)
    {
        using var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8, bufferSize, leaveOpen);
        writer.AutoFlush = true;
        writer.Write(text);
    }
    public static void WriteAllTextAsync(this Stream stream, string text,
        Encoding? encoding = null, int bufferSize = 4096, bool leaveOpen = false)
    {
        using var writer = new StreamWriter(stream, encoding ?? Encoding.UTF8, bufferSize, leaveOpen);
        writer.AutoFlush = true;
        writer.Write(text);
    }
}