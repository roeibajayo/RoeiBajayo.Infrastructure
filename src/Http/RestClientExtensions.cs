using RoeiBajayo.Infrastructure.Http.Models;
using RoeiBajayo.Infrastructure.Text;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Http;

public static class RestClientExtensions
{
    #region Get
    private static async Task<string> GetStringAsync(this IRestClient client, string url, RestCallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<T?> GetAsync<T>(this IRestClient client, string url, RestCallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<Stream> GetStreamAsync(this IRestClient client, string url, RestCallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
    public static async Task GetDownloadAsync(this IRestClient client, string url, string path, RestCallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(url, options, cancellationToken);
        await DownloadAsync(response, url, path, cancellationToken);
    }
    public static async Task<HtmlTag> GetHtmlAsync(this IRestClient client, string url, RestCallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var stream = await client.GetStreamAsync(url, options, cancellationToken);
        return await HtmlTagParser.ParseAsync(stream);
    }

    #endregion

    #region Post

    public static async Task<T?> PostAsync<T>(this IRestClient client, string url, RestCallOptions options, CancellationToken cancellationToken = default)
    {
        var response = await client.PostStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<HtmlTag> PostHtmlAsync(this IRestClient client, string url, RestCallOptions options, CancellationToken cancellationToken = default)
    {
        var stream = await client.PostStreamAsync(url, options, cancellationToken);
        return await HtmlTagParser.ParseAsync(stream);
    }
    public static async Task PostDownloadAsync(this IRestClient client, string url, string path, RestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        await DownloadAsync(response, url, path, cancellationToken);
    }
    private static async Task<string> PostStringAsync(this IRestClient client, string url, RestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<Stream> PostStreamAsync(this IRestClient client, string url, RestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public static async Task<T?> PostAsync<T>(this IRestClient client, string url, StreamBodyRestCallOptions options,
        CancellationToken cancellationToken = default)
    {
        var response = await client.PostStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<HtmlTag> PostHtmlAsync(this IRestClient client, string url, StreamBodyRestCallOptions options,
        CancellationToken cancellationToken = default)
    {
        var stream = await client.PostStreamAsync(url, options, cancellationToken);
        return await HtmlTagParser.ParseAsync(stream);
    }
    public static async Task PostDownloadAsync(this IRestClient client, string url, string path, StreamBodyRestCallOptions options,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        await DownloadAsync(response, url, path, cancellationToken);
    }
    private static async Task<string> PostStringAsync(this IRestClient client, string url, StreamBodyRestCallOptions options,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<Stream> PostStreamAsync(this IRestClient client, string url, StreamBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public static async Task<T?> PostAsync<T>(this IRestClient client, string url, JsonBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        var response = await client.PostStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<HtmlTag> PostHtmlAsync(this IRestClient client, string url, JsonBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        var stream = await client.PostStreamAsync(url, options, cancellationToken);
        return await HtmlTagParser.ParseAsync(stream);
    }
    public static async Task PostDownloadAsync(this IRestClient client, string url, string path, JsonBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        await DownloadAsync(response, url, path, cancellationToken);
    }
    private static async Task<string> PostStringAsync(this IRestClient client, string url, JsonBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<Stream> PostStreamAsync(this IRestClient client, string url, JsonBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public static async Task<T?> PostAsync<T>(this IRestClient client, string url, TextBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        var response = await client.PostStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<HtmlTag> PostHtmlAsync(this IRestClient client, string url, TextBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        var stream = await client.PostStreamAsync(url, options, cancellationToken);
        return await HtmlTagParser.ParseAsync(stream);
    }
    public static async Task PostDownloadAsync(this IRestClient client, string url, string path, TextBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        await DownloadAsync(response, url, path, cancellationToken);
    }
    private static async Task<string> PostStringAsync(this IRestClient client, string url, TextBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<Stream> PostStreamAsync(this IRestClient client, string url, TextBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public static async Task<T?> PostAsync<T>(this IRestClient client, string url, MultipartFormDataRestCallOptions options, CancellationToken cancellationToken = default)
    {
        var response = await client.PostStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<HtmlTag> PostHtmlAsync(this IRestClient client, string url, FormDataRestCallOptions options, CancellationToken cancellationToken = default)
    {
        var stream = await client.PostStreamAsync(url, options, cancellationToken);
        return await HtmlTagParser.ParseAsync(stream);
    }
    public static async Task PostDownloadAsync(this IRestClient client, string url, string path, MultipartFormDataRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        await DownloadAsync(response, url, path, cancellationToken);
    }
    private static async Task<string> PostStringAsync(this IRestClient client, string url, MultipartFormDataRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<Stream> PostStreamAsync(this IRestClient client, string url, MultipartFormDataRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    public static async Task<T?> PostAsync<T>(this IRestClient client, string url, FormDataRestCallOptions options, CancellationToken cancellationToken = default)
    {
        var response = await client.PostStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<HtmlTag> PostHtmlAsync(this IRestClient client, string url, MultipartFormDataRestCallOptions options, CancellationToken cancellationToken = default)
    {
        var stream = await client.PostStreamAsync(url, options, cancellationToken);
        return await HtmlTagParser.ParseAsync(stream);
    }
    public static async Task PostDownloadAsync(this IRestClient client, string url, string path, FormDataRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        await DownloadAsync(response, url, path, cancellationToken);
    }
    private static async Task<string> PostStringAsync(this IRestClient client, string url, FormDataRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<Stream> PostStreamAsync(this IRestClient client, string url, FormDataRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    #endregion

    #region Put

    private static async Task<string> PutStringAsync(this IRestClient client, string url, StreamBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<T?> PutAsync<T>(this IRestClient client, string url, StreamBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        var response = await client.PutStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<Stream> PutStreamAsync(this IRestClient client, string url, StreamBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
    private static async Task<string> PutStringAsync(this IRestClient client, string url, JsonBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<T?> PutAsync<T>(this IRestClient client, string url, JsonBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        var response = await client.PutStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<Stream> PutStreamAsync(this IRestClient client, string url, JsonBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
    private static async Task<string> PutStringAsync(this IRestClient client, string url, TextBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<T?> PutAsync<T>(this IRestClient client, string url, TextBodyRestCallOptions options,
        CancellationToken cancellationToken = default)
    {
        var response = await client.PutStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<Stream> PutStreamAsync(this IRestClient client, string url, TextBodyRestCallOptions options,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    #endregion

    #region Patch

    private static async Task<string> PatchStringAsync(this IRestClient client, string url, StreamBodyRestCallOptions options,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.PatchAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<T?> PatchAsync<T>(this IRestClient client, string url, StreamBodyRestCallOptions options,
        CancellationToken cancellationToken = default)
    {
        var response = await client.PatchStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<Stream> PatchStreamAsync(this IRestClient client, string url, StreamBodyRestCallOptions options,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.PatchAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
    private static async Task<string> PatchStringAsync(this IRestClient client, string url, JsonBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PatchAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<T?> PatchAsync<T>(this IRestClient client, string url, JsonBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        var response = await client.PatchStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<Stream> PatchStreamAsync(this IRestClient client, string url, JsonBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PatchAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
    private static async Task<string> PatchStringAsync(this IRestClient client, string url, TextBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PatchAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<T?> PatchAsync<T>(this IRestClient client, string url, TextBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        var response = await client.PatchStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<Stream> PatchStreamAsync(this IRestClient client, string url, TextBodyRestCallOptions options, CancellationToken cancellationToken = default)
    {
        using var response = await client.PatchAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    #endregion

    #region Delete

    private static async Task<string> DeleteStringAsync(this IRestClient client, string url, StreamBodyRestCallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<T?> DeleteAsync<T>(this IRestClient client, string url, StreamBodyRestCallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<Stream> DeleteStreamAsync(this IRestClient client, string url, StreamBodyRestCallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
    private static async Task<string> DeleteStringAsync(this IRestClient client, string url, JsonBodyRestCallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<T?> DeleteAsync<T>(this IRestClient client, string url, JsonBodyRestCallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<Stream> DeleteStreamAsync(this IRestClient client, string url, JsonBodyRestCallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }
    private static async Task<string> DeleteStringAsync(this IRestClient client, string url, TextBodyRestCallOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(url, options, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        return responseBody;
    }
    public static async Task<T?> DeleteAsync<T>(this IRestClient client, string url, TextBodyRestCallOptions? options = null, CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteStringAsync(url, options, cancellationToken);
        return JsonParse<T>(response);
    }
    public static async Task<Stream> DeleteStreamAsync(this IRestClient client, string url, TextBodyRestCallOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(url, options, cancellationToken);
        return await response.Content.ReadAsStreamAsync(cancellationToken);
    }

    #endregion

    private static T? JsonParse<T>(string response)
    {
        if (response is null)
            return default;

        if (typeof(T) == typeof(string))
            return (T)(object)response;

        try
        {

            if (typeof(T) == typeof(JsonElement))
            {
                var value = JsonDocument.Parse(response).RootElement;
                return (T)(object)value;
            }

            if (typeof(T) == typeof(DateTime))
            {
                var value = DateTime.Parse(response);
                return (T)(object)value;
            }

            return JsonSerializer.Deserialize<T>(response, DefaultJsonSerializerOptions);
        }
        catch (Exception ex)
        {
            throw new Exception("failed to parse json: " + response, ex);
        }
    }
    private static async Task DownloadAsync(HttpResponseMessage response, string url, string path, CancellationToken cancellationToken)
    {
        var filename = response.Content.Headers.ContentDisposition?.FileName ?? Path.GetFileName(url.Split('?')[0]);

        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        path = Path.Combine(directory!, filename);

        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await stream.CopyToAsync(fileStream);
    }
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
