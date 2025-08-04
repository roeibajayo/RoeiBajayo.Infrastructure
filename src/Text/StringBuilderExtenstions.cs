using System.IO;

namespace System.Text;

public static class StringBuilderExtenstions
{
    public static void Append(this StringBuilder builder, string value, int startIndex)
    {
        builder.Append(value, startIndex, value.Length - startIndex);
    }

    public static void WriteTo(this StringBuilder builder,
        Stream stream, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;

        using var writer = new StreamWriter(stream, encoding: encoding, leaveOpen: true);
        var chunks = builder.GetChunks();
        foreach (var chunk in chunks)
        {
            writer.Write(chunk);
        }
    }
}
