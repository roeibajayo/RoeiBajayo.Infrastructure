using RoeiBajayo.Infrastructure.DependencyInjection.Interfaces;
using RoeiBajayo.Infrastructure.Http.Models;
using RoeiBajayo.Infrastructure.Repositories.Queues.Throttling;
using RoeiBajayo.Infrastructure.Threads;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace RoeiBajayo.Infrastructure.Http;

internal class RestClient : IRestClient, ISingletonService<IRestClient>
{
    private readonly ICookiesRepository cookies;
    private RestClientOptions options = new();
    private ThrottlingCounter? throttlingCounter = null;
    private readonly ILogger<RestClient> logger;
    private readonly Dictionary<string, HttpClient> clients = [];

    public RestClient(ILogger<RestClient> logger) : this(logger, null)
    {
    }
    public RestClient(ILogger<RestClient> logger, RestClientOptions? options) : this(logger, options, new InMemoryCookiesRepository())
    {
    }
    public RestClient(ILogger<RestClient> logger, RestClientOptions? options, ICookiesRepository cookiesRepository)
    {
        this.logger = logger;
        cookies = cookiesRepository;

        if (options is null)
            return;

        SetOptions(options);
    }

    public RestClientOptions Options => options;
    public void SetOptions(RestClientOptions options)
    {
        this.options = options;

        if (options.Throttling is not null && options.Throttling.Length > 0)
            throttlingCounter = new ThrottlingCounter(options.Throttling);
    }

    public ICookiesRepository Cookies => cookies;

    private HttpClient GetClient(RestCallOptions? options)
    {
        var ignoreBadCertificates = options?.IgnoreBadCertificates ?? this.options?.IgnoreBadCertificates ?? false;
        var proxy = options?.Proxy ?? this.options?.Proxy;

        var clientName = $"{ignoreBadCertificates}_{proxy}";

        if (clients.TryGetValue(clientName, out var client))
            return client;

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false
        };

        if (ignoreBadCertificates)
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        if (!string.IsNullOrEmpty(proxy))
        {
            handler.Proxy = new WebProxy(proxy);
            handler.UseProxy = true;
        }

        client = new HttpClient(handler);
        lock (clients)
        {
            clients[clientName] = client;
        }
        return client;
    }

    async Task<HttpResponseMessage> IRestClient.GetAsync(string url, RestCallOptions? callOptions, CancellationToken cancellationToken)
    {
        url = FormatUrl(url, options?.BaseUrl, callOptions);
        return await FetchResponseAsync(callOptions, new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url)
        }, callOptions?.Retries ?? 0, cancellationToken);
    }

    async Task<HttpResponseMessage> IRestClient.PostAsync(string url, RestCallOptions callOptions, CancellationToken cancellationToken)
    {
        url = FormatUrl(url, options?.BaseUrl, callOptions);
        return await FetchResponseAsync(callOptions, new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(url)
        }, callOptions.Retries, cancellationToken);
    }
    async Task<HttpResponseMessage> IRestClient.PostAsync(string url, BodyRestCallOptions callOptions, CancellationToken cancellationToken)
    {
        url = FormatUrl(url, options?.BaseUrl, callOptions);
        return await FetchResponseAsync(callOptions, new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(url),
            Content = GetContent(callOptions)
        }, callOptions.Retries, cancellationToken);
    }
    async Task<HttpResponseMessage> IRestClient.PostAsync(string url, MultipartFormDataRestCallOptions callOptions, CancellationToken cancellationToken)
    {
        try
        {
            url = FormatUrl(url, options?.BaseUrl, callOptions);

            var formData = new MultipartFormDataContent();

            foreach (var item in callOptions.FormData)
            {
                if (item.Value.IsFile)
                {
                    var key = item.Key.RemovePathInvalidChars();
                    var filename = item.Value.Filename!.RemovePathInvalidChars();
                    var fileContent = new StreamContent(item.Value.Stream!);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(item.Value.ContentType!);
                    formData.Add(fileContent, key, filename);
                }
                else
                {
                    formData.Add(new StringContent(item.Value.Value is string ?
                        (item.Value.Value as string)! :
                        item.Value.Value == null ? "" :
                        JsonSerializer.Serialize(item.Value)), item.Key);
                }
            }

            return await FetchResponseAsync(callOptions, new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = formData
            }, callOptions.Retries, cancellationToken);
        }
        finally
        {
            if (!callOptions.LeaveOpen)
            {
                foreach (var file in callOptions.FormData.Values.Where(x => x.IsFile))
                {
                    try
                    {
                        file.Stream!.Dispose();
                    }
                    catch { }
                }
            }
        }
    }
    async Task<HttpResponseMessage> IRestClient.PostAsync(string url, FormDataRestCallOptions callOptions, CancellationToken cancellationToken)
    {
        url = FormatUrl(url, options?.BaseUrl, callOptions);

        var data = callOptions.FormData.Select(item =>
            new KeyValuePair<string, string>(item.Key, item.Value is string ?
                        (item.Value as string)! :
                        (item.Value is null ? "" :
                        JsonSerializer.Serialize(item.Value))));
        var formData = new FormUrlEncodedContent(data);

        return await FetchResponseAsync(callOptions, new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(url),
            Content = formData
        }, callOptions.Retries, cancellationToken);
    }

    async Task<HttpResponseMessage> IRestClient.PutAsync(string url, RestCallOptions? callOptions, CancellationToken cancellationToken)
    {
        url = FormatUrl(url, options?.BaseUrl, callOptions);
        return await FetchResponseAsync(callOptions, new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri(url)
        }, callOptions?.Retries ?? 0, cancellationToken);
    }
    async Task<HttpResponseMessage> IRestClient.PutAsync(string url, BodyRestCallOptions? callOptions, CancellationToken cancellationToken)
    {
        url = FormatUrl(url, options?.BaseUrl, callOptions);
        return await FetchResponseAsync(callOptions, new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri(url),
            Content = GetContent(callOptions)
        }, callOptions?.Retries ?? 0, cancellationToken);
    }

    async Task<HttpResponseMessage> IRestClient.PatchAsync(string url, RestCallOptions? callOptions, CancellationToken cancellationToken)
    {
        url = FormatUrl(url, options?.BaseUrl, callOptions);
        return await FetchResponseAsync(callOptions, new HttpRequestMessage
        {
            Method = HttpMethod.Patch,
            RequestUri = new Uri(url)
        }, callOptions?.Retries ?? 0, cancellationToken);
    }
    async Task<HttpResponseMessage> IRestClient.PatchAsync(string url, BodyRestCallOptions? callOptions, CancellationToken cancellationToken)
    {
        url = FormatUrl(url, options?.BaseUrl, callOptions);
        return await FetchResponseAsync(callOptions, new HttpRequestMessage
        {
            Method = HttpMethod.Patch,
            RequestUri = new Uri(url),
            Content = GetContent(callOptions)
        }, callOptions?.Retries ?? 0, cancellationToken);
    }

    async Task<HttpResponseMessage> IRestClient.DeleteAsync(string url, RestCallOptions? callOptions, CancellationToken cancellationToken)
    {
        url = FormatUrl(url, options?.BaseUrl, callOptions);
        return await FetchResponseAsync(callOptions, new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(url)
        }, callOptions?.Retries ?? 0, cancellationToken);
    }
    async Task<HttpResponseMessage> IRestClient.DeleteAsync(string url, BodyRestCallOptions? callOptions, CancellationToken cancellationToken)
    {
        url = FormatUrl(url, options?.BaseUrl, callOptions);
        return await FetchResponseAsync(callOptions, new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(url),
            Content = GetContent(callOptions)
        }, callOptions?.Retries ?? 0, cancellationToken);
    }

    private static HttpContent? GetContent(BodyRestCallOptions? options)
    {
        if (options is null)
            return null;

        if (options is StreamBodyRestCallOptions streamOptions)
            return new StreamContent(streamOptions.Body!);

        string? body = GetStringBodyString(options);

        var contentType = options.ContentType ?? (options is TextBodyRestCallOptions ? "text/plain" : "application/json");

        return new StringContent(body ?? "",
            Encoding.UTF8,
            contentType);
    }

    private static string? GetStringBodyString(BodyRestCallOptions options)
    {
        if (options is TextBodyRestCallOptions textOptions)
            return textOptions.Text;

        var jsonOptions = (options as JsonBodyRestCallOptions)!;
        if (jsonOptions.Json is null)
            return null;

        return JsonSerializer.Serialize(jsonOptions.Json,
            jsonOptions.JsonSerializerOptions ??
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
    }

    private static string FormatUrl(string url, string? baseUrl, RestCallOptions? options)
    {
        if (options?.Querystring is not null && options.Querystring.Count > 0)
        {
            var result = new StringBuilder();
            result.Append(url);

            var firstQuery = !url.Contains('?');
            foreach (var query in options.Querystring)
            {
                result.Append(firstQuery ? '?' : '&');
                result.Append(query.Key);
                result.Append('=');

                var value = query.Value;

                if (value is null)
                    continue;

                if (value is DateTime date)
                    value = date.ToString("yyyy-MM-ddTHH:mm:ss");

                result.Append(HttpUtility.UrlEncode(value?.ToString()));
                firstQuery = false;
            }
            url = result.ToString();
        }

        if (!string.IsNullOrEmpty(options?.BaseUrl))
        {
            baseUrl = options.BaseUrl;
        }

        return baseUrl == null ? url : (baseUrl.TrimEnd('/') + '/' + url.TrimStart('/'));
    }
    private async Task<HttpResponseMessage> FetchResponseAsync(RestCallOptions? options,
        HttpRequestMessage request, int retiresLeft, CancellationToken cancellationToken, int? redirectsLeft = null)
    {
        var timeout = options?.Timeout ??
            this.options?.Timeout ??
            TimeSpan.FromSeconds(60);

        if (cancellationToken.IsCancellationRequested)
            throw new TaskCanceledException();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);
        cancellationToken = cts.Token;

        if (options?.UseOnlineProxy ?? this.options?.UseOnlineProxy == true)
        {
            return await FetchResponseUsingOnlineProxyAsync(options, request, cancellationToken);
        }

        if (throttlingCounter is not null)
        {
            await throttlingCounter.WaitForEnqueueAsync(cancellationToken);
        }

        var body = PrepareRequest(options, request);
        var now = DateTime.UtcNow;
        var index = new Random().Next(1, 100);
        logger?.LogTrace("#{index} {method} {url} {body}", index, request.Method, request.RequestUri, body);

        return await Tasks.StartWithTimeout(async () =>
        {
            try
            {
                var client = GetClient(options);
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

                try
                {
                    if ((response.Headers?.TryGetValues("Content-Type", out var contentTypes) ?? false) &&
                        (contentTypes?.Any(x => x.Contains("application/problem+json")) ?? false))
                    {
                        var content = await response.Content.ReadAsStringAsync(cancellationToken);
                        logger?.LogTrace("#{index} response: {content}", index, content);

                        var problem = JsonNode.Parse(content)!.Root;
                        var extensions = problem.AsObject().ToDictionary(x => x.Key, x => x.Value);
                        extensions.Remove("type");
                        extensions.Remove("title");
                        extensions.Remove("detail");
                        extensions.Remove("instance");
                        throw new ProblemJsonException(
                            (int)response.StatusCode,
                            problem["type"]?.ToString() ?? "",
                            problem["title"]?.ToString() ?? "",
                            problem["detail"]?.ToString() ?? "",
                            problem["instance"]?.ToString() ?? "",
                            extensions);
                    }

                    if ((options?.SkipEnsureSuccessStatusCode is not null ?
                            options.SkipEnsureSuccessStatusCode != true :
                            this.options?.SkipEnsureSuccessStatusCode != true) &&
                        !response.IsSuccessStatusCode &&
                        response.StatusCode != HttpStatusCode.Found &&
                        response.StatusCode != HttpStatusCode.Moved &&
                        response.StatusCode != HttpStatusCode.MovedPermanently)
                    {
                        throw new Exception("Invalid status code: " + response.StatusCode);
                    }

                    if (options?.UseCookies ?? this.options?.UseCookies == true)
                    {
                        if (response.Headers?.TryGetValues("Set-Cookie", out var setCookies) ?? false)
                        {
                            var repository = options?.Cookies ?? this.options?.Cookies ?? cookies;
                            var host = request.RequestUri!.Host;

                            foreach (var setCookie in setCookies)
                            {
                                var parts = setCookie.Split(';');
                                var keyValue = parts[0];
                                var splitIndex = keyValue.IndexOf('=');
                                var key = keyValue[0..splitIndex];
                                var value = keyValue[(splitIndex + 1)..];
                                repository.Add(host, key, value);
                            }
                        }
                    }

                    if (response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.Moved or HttpStatusCode.MovedPermanently or
                        HttpStatusCode.RedirectKeepVerb or HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect &&
                        (options?.AutoRedirect ?? this.options?.AutoRedirect ?? true))
                    {
                        if (redirectsLeft == 0)
                            throw new Exception("Too many redirects");

                        var location = response.Headers?.Location?.ToString();
                        if (location is not null)
                        {
                            var url = location.StartsWith('/') ?
                                    new Uri(request.RequestUri!.GetLeftPart(UriPartial.Authority) + location) :
                                    new Uri(location);

                            redirectsLeft ??= 3;

                            return await FetchResponseAsync(options,
                                new HttpRequestMessage(HttpMethod.Get, url), retiresLeft, cancellationToken, redirectsLeft--);
                        }
                    }


                    var took = (DateTime.UtcNow - now).TotalMilliseconds;

                    logger?.LogTrace("#{index} took {time}{timeUnit} with status code {statusCode}",
                        index,
                        took < 1000 ? took : Math.Round(took / 1000, 2, MidpointRounding.AwayFromZero),
                        took < 1000 ? "ms" : "s",
                        response.StatusCode);

                    return response;
                }
                catch (Exception e)
                {
                    var message = "Invalid response";
                    string? content = null;

                    if ((response.Headers.TryGetValues("Content-Length", out var contentLength) &&
                            long.Parse(contentLength.First()) < 6291456) || //6mb
                        !response.Headers.TryGetValues("Content-Type", out var contentTypes) ||
                            contentTypes.Any(x => x.Contains("text") || x.Contains("json")))
                    {
                        content = await response.Content.ReadAsStringAsync(cancellationToken);
                        message += ": " + content;
                    }

                    var logLevel = retiresLeft > 0 ? LogLevel.Warning : LogLevel.Error;
                    logger?.Log(logLevel, e, "Failed to fetch {url} with status code {statusCode} and content: {content}",
                        request.RequestUri, response.StatusCode, content);

                    throw new InvalidResponseException(message, e)
                    {
                        Request = request,
                        Response = response,
                        Content = content
                    };
                }
            }
            catch (Exception e)
            {
                if (retiresLeft > 0)
                {
                    logger?.LogTrace(e, "Failed to fetch {url}, retrying", request.RequestUri);

                    if (options?.WaitMsBetweenRetries > 0)
                        await Task.Delay(options.WaitMsBetweenRetries);

                    return await FetchResponseAsync(options,
                        new HttpRequestMessage(request.Method, request.RequestUri)
                        {
                            Content = request.Content
                        }, retiresLeft - 1, cancellationToken);
                }

                if (e is InvalidResponseException)
                    throw;

                throw new InvalidResponseException("Invalid response", e)
                {
                    Request = request
                };
            }
        }, cancellationToken);
    }

    private string? PrepareRequest(RestCallOptions? options, HttpRequestMessage request)
    {
        var fakeUserAgent = options?.FakeUserAgent;

        if (options?.Headers is not null)
        {
            foreach (var header in options.Headers)
            {
                if (string.IsNullOrWhiteSpace(header.Key) || string.IsNullOrWhiteSpace(header.Value))
                    continue;

                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (this.options?.Headers is not null)
        {
            foreach (var header in this.options.Headers)
            {
                if (string.IsNullOrWhiteSpace(header.Key) || string.IsNullOrWhiteSpace(header.Value))
                    continue;

                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (options?.UseCookies ?? this.options?.UseCookies == true)
        {
            var repository = options?.Cookies ?? this.options?.Cookies ?? this.cookies;
            var host = request.RequestUri!.Host;
            var cookies = repository.Get(host);
            var cookie = string.Join("; ", cookies.Select(x => $"{x.Key}={x.Value}"));

            if (!string.IsNullOrEmpty(cookie))
                request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        if (options?.DefaultHeaders ?? this.options?.DefaultHeaders == true)
        {
            fakeUserAgent ??= this.options?.FakeUserAgent ?? true;

            request.Headers.TryAddWithoutValidation("Accept", "*/*");
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "deflate");
            request.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9,he-IL;q=0.8,he;q=0.7");
            request.Headers.TryAddWithoutValidation("Pragma", "no-cache");
            request.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
        }

        if (fakeUserAgent ?? this.options?.FakeUserAgent == true)
        {
            request.Headers.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
        }

        string? body = null;
        if (options is BodyRestCallOptions bodyOptions)
        {
            body = GetStringBodyString(bodyOptions);
        }
        else if (options is MultipartFormDataRestCallOptions formDataOptions)
        {
            body = string.Join(Environment.NewLine, formDataOptions.FormData.Select(x => $"{x.Key}={x.Value}"));
        }
        return body;
    }

    private async Task<HttpResponseMessage> FetchResponseUsingOnlineProxyAsync(RestCallOptions? options,
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get)
            throw new NotSupportedException("Proxy is only supported for GET requests");

        var cookie = await GetProxyCookieAsync(cancellationToken);

        return await (this as IRestClient).PostAsync("https://proxy-us.steganos.com/includes/process.php",
            new FormDataRestCallOptions
            {
                Querystring = new Dictionary<string, object?>
                {
                    { "action", "update" }
                },
                FormData = new Dictionary<string, object>
                {
                    { "u", request.RequestUri!.ToString() },
                    { "wp_location", "https://proxy-us.steganos.com/includes/process.php?action=update" }
                },
                Headers = new Dictionary<string, string>
                {
                    { "Cookie", cookie ?? "" },
                    { "Referer", "https://www.steganos.com/" }
                },
                SkipEnsureSuccessStatusCode = options?.SkipEnsureSuccessStatusCode,
                AutoRedirect = true,
            }, cancellationToken);
    }

    private string? proxyCookie = null;
    private async Task<string?> GetProxyCookieAsync(CancellationToken cancellationToken)
    {
        if (proxyCookie is not null)
            return proxyCookie;

        var response = await (this as IRestClient).GetAsync("https://www.steganos.com/en/free-online-web-proxy", cancellationToken: cancellationToken);
        proxyCookie = response.Headers.GetValues("Set-Cookie").FirstOrDefault();
        return proxyCookie;
    }
}
