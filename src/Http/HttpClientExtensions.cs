using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace System.Net.Http;

public static class HttpClientExtensions
{
    public static async Task<HttpResponseMessage> PostJsonAsync(this HttpClient client,
        string url, object data)
    {
        client.DefaultRequestHeaders
              .Accept
              .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var json = JsonSerializer.Serialize(data);
        var response = await client.PostAsync(url,
            new StringContent(json, Encoding.UTF8, "application/json"));
        return response;
    }

    public static async Task<HttpResponseMessage> PutJsonAsync(this HttpClient client,
        string url, object data)
    {
        client.DefaultRequestHeaders
              .Accept
              .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var json = JsonSerializer.Serialize(data);
        var response = await client.PutAsync(url,
            new StringContent(json, Encoding.UTF8, "application/json"));
        return response;
    }
}
