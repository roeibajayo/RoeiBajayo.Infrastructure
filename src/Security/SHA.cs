using System;
using System.Security.Cryptography;
using System.Text;

namespace RoeiBajayo.Infrastructure.Security;

public static class SHA
{
    public static string HashTo256(string text)
    {
        using var alg = SHA256.Create();
        return Hash(alg, text);
    }
    public static string HashTo512(string text)
    {
        using var alg = SHA512.Create();
        return Hash(alg, text);
    }

    private static string Hash(HashAlgorithm alg, string text)
    {
        var hashedBytes = alg.ComputeHash(Encoding.UTF8.GetBytes(text));
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
    }

    public static string ToSHA256(this string text) =>
        HashTo256(text);
    public static string ToSHA512(this string text) =>
        HashTo512(text);
}
