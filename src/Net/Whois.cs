using Infrastructure.Utils.DependencyInjection.Interfaces;
using Infrastructure.Utils.Http;
using Infrastructure.Utils.Net.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Utils.Net;

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
