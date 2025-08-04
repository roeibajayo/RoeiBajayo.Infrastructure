using RoeiBajayo.Infrastructure.Http.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Http;

public interface IRestClient
{
    void SetOptions(RestClientOptions options);
    RestClientOptions Options { get; }
    ICookiesRepository Cookies { get; }

    Task<HttpResponseMessage> DeleteAsync(string url, RestCallOptions? options = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> DeleteAsync(string url, BodyRestCallOptions? options = null, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> GetAsync(string url, RestCallOptions? options = null, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> PatchAsync(string url, RestCallOptions? options = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PatchAsync(string url, BodyRestCallOptions? options = null, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> PostAsync(string url, RestCallOptions options, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PostAsync(string url, BodyRestCallOptions options, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PostAsync(string url, FormDataRestCallOptions options, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PostAsync(string url, MultipartFormDataRestCallOptions options, CancellationToken cancellationToken = default);

    Task<HttpResponseMessage> PutAsync(string url, RestCallOptions? options = null, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> PutAsync(string url, BodyRestCallOptions? options = null, CancellationToken cancellationToken = default);
}
