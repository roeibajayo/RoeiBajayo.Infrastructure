using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Utils.Security;

public static class AES
{
    //key must be 16, 24 or 32 length
    //ivKey is 16 length

    public static string Encrypt(this string input, string key, string? ivKey = null)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var ivKeyBytes = ivKey == null ? null : Encoding.UTF8.GetBytes(ivKey);
        return Encrypt(input, keyBytes, ivKeyBytes);
    }
    public static string Encrypt(this string input, byte[] key, byte[]? ivKey = null)
    {
        using var aesAlg = Aes.Create();
        var iv = ivKey ?? aesAlg.IV;

        using var encryptor = aesAlg.CreateEncryptor(key, iv);
        using var msEncrypt = new MemoryStream();

        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(input);
        }

        var decryptedContent = msEncrypt.ToArray();
        var result = new byte[iv.Length + decryptedContent.Length];

        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

        return Convert.ToBase64String(result);
    }

    public static string Decrypt(this string input, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        return Decrypt(input, keyBytes);
    }
    public static string Decrypt(this string input, byte[] key)
    {
        var fullCipher = Convert.FromBase64String(input);

        var iv = new byte[16];
        var cipher = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, fullCipher.Length - iv.Length);

        using var aesAlg = Aes.Create();
        using var decryptor = aesAlg.CreateDecryptor(key, iv);
        using var msDecrypt = new MemoryStream(cipher);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }


}