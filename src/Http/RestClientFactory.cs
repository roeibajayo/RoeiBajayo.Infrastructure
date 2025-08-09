using MemoryCore;
using Microsoft.Extensions.Logging;
using RoeiBajayo.Infrastructure.DependencyInjection.Interfaces;
using RoeiBajayo.Infrastructure.Http.Models;
using System;

namespace RoeiBajayo.Infrastructure.Http;

internal class RestClientFactory(IMemoryCore cache, ILogger<RestClient> logger)
    : IRestClientFactory, ISingletonService<IRestClientFactory>
{
    private static string GetCacheKey(string? name) =>
        string.Concat("__REST_CLIENT_", name ?? "__default");

    public IRestClient GetClient(string? name = null, RestClientOptions? defaultOptions = null)
    {
        var key = GetCacheKey(name);
        return GetOrCreate(key, defaultOptions);
    }
    public void ResetClient(string? name = null) =>
        cache.Remove(GetCacheKey(name));

    private RestClient GetOrCreate(string key, RestClientOptions? defaultOptions)
    {
        return cache.TryGetOrAdd(key, () => Create(defaultOptions), TimeSpan.FromMinutes(60))!;
    }
    private RestClient Create(RestClientOptions? defaultOptions)
    {
        var client = new RestClient(logger);
        if (defaultOptions != null)
        {
            client.SetOptions(defaultOptions);
        }
        return client;
    }
}
