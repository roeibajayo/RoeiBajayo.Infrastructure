using Microsoft.AspNetCore.Http;

namespace System.Net.Http;

public static class HttpContextExtensions
{
    public static string GetIp(this HttpContext httpContext)
    {
        var request = httpContext.Request;

        // handle standardized 'Forwarded' header
        string? forwarded = request.Headers["Forwarded"];
        if (!string.IsNullOrEmpty(forwarded))
        {
            foreach (string segment in forwarded.Split(',')[0].Split(';'))
            {
                string[] pair = segment.Trim().Split('=');
                if (pair.Length == 2 && pair[0].Equals("for", StringComparison.OrdinalIgnoreCase))
                {
                    string ip = pair[1].Trim('"');

                    // IPv6 addresses are always enclosed in square brackets
                    int left = ip.IndexOf('['), right = ip.IndexOf(']');
                    if (left == 0 && right > 0)
                    {
                        return ip[1..right];
                    }

                    // strip port of IPv4 addresses
                    int colon = ip.IndexOf(':');
                    if (colon != -1)
                    {
                        return ip[..colon];
                    }

                    // this will return IPv4, "unknown", and obfuscated addresses
                    return ip;
                }
            }
        }

        // handle non-standardized 'X-Forwarded-For' header
        string? xForwardedFor = request.Headers["X-Forwarded-For"];
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0];
        }


        if (request.Headers.TryGetValue("HTTP_X_FORWARDED_FOR", out var ips))
        {
            if (!string.IsNullOrEmpty(ips))
            {
                string[] addresses = (ips + "").Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }
        }

        return request.HttpContext.Connection.RemoteIpAddress.ToString();
    }

    public static string GetUserAgent(this HttpContext httpContext)
    {
        return httpContext.Request.Headers["User-Agent"].ToString() ?? "unknown";
    }
}
