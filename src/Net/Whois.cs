using RoeiBajayo.Infrastructure.DependencyInjection.Interfaces;
using RoeiBajayo.Infrastructure.Http;
using RoeiBajayo.Infrastructure.Net.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Net;

public class Whois(IRestClient client) : ISingletonService
{
    private const string IPINFO_TOKEN = "6cfa02b3d85871";

    public async Task<GeoLocationResult> GetCountryAsync(string? ipAddress = null,
        string ipInfoToken = IPINFO_TOKEN)
    {
        return (await client
            .GetAsync<GeoLocationResult>($"https://ipinfo.io/{ipAddress}", new Http.Models.RestCallOptions
            {
                Querystring = new Dictionary<string, object?>() { { "token", ipInfoToken } }
            }))!;
    }
}
