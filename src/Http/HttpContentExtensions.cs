using RoeiBajayo.Infrastructure.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace System.Net.Http;

public static class HttpContentExtensions
{
    private static readonly JsonSerializerOptions DEFAULT_OPTIONS = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public static async Task<T?> ReadAsync<T>(this HttpContent content,
            JsonSerializerOptions? options = null)
    {
        var response = await content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<T>(response, options ?? DEFAULT_OPTIONS);
    }

    public static async Task<HtmlTag> ReadAsHtmlAsync(this HttpContent content) =>
        await HtmlTagParser.ParseAsync(await content.ReadAsStreamAsync());

}
