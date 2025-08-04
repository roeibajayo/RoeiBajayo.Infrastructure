using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace System.IO;

public static class StreamReaderExtensions
{
    public static IEnumerable<string> ToChunks(this StreamReader reader, int charsToRead = 4096)
    {
        if (reader.EndOfStream)
        {
            yield break;
        }

        var buffer = new char[charsToRead];
        int read;
        while ((read = reader.ReadBlock(buffer, 0, charsToRead)) > 0)
        {
            yield return new string(buffer, 0, read);
        }
    }
    public static async IAsyncEnumerable<string> ToChunksAsync(this StreamReader reader, int charsToRead = 4096,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (reader.EndOfStream)
        {
            yield break;
        }

        var buffer = new char[charsToRead];
        int read;
        while (!cancellationToken.IsCancellationRequested && (read = await reader.ReadAsync(buffer, 0, charsToRead)) > 0)
        {
            yield return new string(buffer, 0, read);
        }
    }

    public static IEnumerable<string> Split(this StreamReader reader, char splitAt, int charsToRead = 4096) =>
        Split(reader, [splitAt], charsToRead);
    public static IEnumerable<string> Split(this StreamReader reader, char[] splitAtAny, int charsToRead = 4096)
    {
        var sb = new StringBuilder();
        foreach (var block in reader.ToChunks(charsToRead))
        {
            int lastCurrentIndex = 0;
            int currentIndex;
            while ((currentIndex = block.IndexOfAny(splitAtAny, lastCurrentIndex)) >= 0)
            {
                if (currentIndex > 0)
                {
                    sb.Append(block[lastCurrentIndex..currentIndex]);
                }

                yield return sb.ToString();

                //reset
                sb.Clear();

                if (currentIndex + 1 >= block.Length)
                {
                    lastCurrentIndex = block.Length;
                    break;
                }

                lastCurrentIndex = currentIndex + 1;
            }

            if (block.Length > lastCurrentIndex)
            {
                sb.Append(block.AsSpan(lastCurrentIndex));
            }
        }

        if (sb.Length > 0)
            yield return sb.ToString();
    }
}