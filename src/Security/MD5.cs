using System.IO;
using System.Text;

namespace Infrastructure.Utils.Security;

public static class MD5
{
    public static string Hash(string input)
    {
        return Hash(Encoding.ASCII.GetBytes(input));
    }

    public static string HashFile(string path)
    {
        using var file = File.OpenRead(path);
        return Hash(file);
    }

    public static string Hash(Stream input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(input);
        return ComputedHashToHash(hash);
    }

    public static string Hash(byte[] data)
    {
        var hash = System.Security.Cryptography.MD5.HashData(data);
        return ComputedHashToHash(hash);
    }

    private static string ComputedHashToHash(byte[] hash)
    {
        var hashResult = new StringBuilder(hash.Length * 2);
        foreach (byte t in hash)
            hashResult.Append(t.ToString("x2"));
        return hashResult.ToString();
    }

}
