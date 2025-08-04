using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RoeiBajayo.Infrastructure.Files;

public static class Images
{
    public enum ImageFormat
    {
        unknown,
        bmp,
        jpeg,
        gif,
        tiff,
        png
    }

    private static readonly byte[] bmp = Encoding.ASCII.GetBytes("BM");
    private static readonly byte[] gif = Encoding.ASCII.GetBytes("GIF");
    private static readonly byte[] png = [137, 80, 78, 71];
    private static readonly byte[] tiff = [73, 73, 42];
    private static readonly byte[] tiff2 = [77, 77, 42];
    private static readonly byte[] jpeg = [255, 216, 255, 224];
    private static readonly byte[] jpeg2 = [255, 216, 255, 225];

    public static ImageFormat GetImageFormat(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var bytes = new byte[4];
        var count = stream.Read(bytes, 0, 4);

        if (count < 4)
            return ImageFormat.unknown;

        return GetImageFormat(bytes);
    }
    public static ImageFormat GetImageFormat(IEnumerable<byte> bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        if (bytes.CountImproved(4) >= 4)
        {
            try
            {
                if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                    return ImageFormat.bmp;

                if (gif.SequenceEqual(bytes.Take(gif.Length)))
                    return ImageFormat.gif;

                if (png.SequenceEqual(bytes.Take(png.Length)))
                    return ImageFormat.png;

                if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                    return ImageFormat.tiff;

                if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                    return ImageFormat.tiff;

                if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                    return ImageFormat.jpeg;

                if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                    return ImageFormat.jpeg;
            }
            catch { }
        }

        return ImageFormat.unknown;
    }
}
