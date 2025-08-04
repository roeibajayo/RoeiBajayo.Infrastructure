using System;

namespace RoeiBajayo.Infrastructure.Security;

public static class SignatureValidator
{
    public enum Algorithm
    {
        MD5,
        AES,
        SHA256,
        SHA512
    }

    private const Algorithm DEFAULT_ALGORITHM = Algorithm.MD5;
    public static string Create(string key, params object[] parameters) => Create(key, Algorithm.MD5, parameters);
    public static string Create(string key, Algorithm algorithm, params object[] parameters)
    {
        var timestamp = DateTime.UtcNow.ToUnixTime();
        var result = string.Concat(timestamp, '.', InternalCreate(key, algorithm, timestamp, parameters));
        if (algorithm == DEFAULT_ALGORITHM)
            return result;
        return string.Concat(algorithm, '.', result);
    }
    private static string InternalCreate(string key, Algorithm algorithm, long timestamp, params object[] parameters)
    {
        var input = string.Concat(timestamp, '.', string.Join('.', parameters));
        return algorithm switch
        {
            Algorithm.MD5 => MD5.Hash(key + input),
            Algorithm.AES => input.Encrypt(key),
            Algorithm.SHA256 => (key + input).ToSHA256(),
            Algorithm.SHA512 => (key + input).ToSHA512(),
            _ => throw new NotImplementedException()
        };
    }

    public static bool Validate(string key, string signature, TimeSpan timestampLifetime, params object[] parameters)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        if (string.IsNullOrWhiteSpace(signature))
            return false;

        var parts = signature.Split('.');
        if (parts.Length < 2 || parts.Length > 3)
            return false;

        var timestamp = long.Parse(parts[^2]);

        if (DateTime.UtcNow.ToUnixTime() - timestamp > timestampLifetime.TotalSeconds)
            return false;

        var algorithm = parts.Length == 2 ?
            DEFAULT_ALGORITHM :
            (Algorithm)Enum.Parse(typeof(Algorithm), parts[0]);

        return signature == Create(key, algorithm, timestamp, parameters);
    }

}
