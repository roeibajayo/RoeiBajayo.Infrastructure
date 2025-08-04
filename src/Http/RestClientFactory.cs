using RoeiBajayo.Infrastructure.DependencyInjection.Interfaces;
using RoeiBajayo.Infrastructure.Http.Models;
using MemoryCore;
using Microsoft.Extensions.Logging;
using System;

namespace RoeiBajayo.Infrastructure.Http;

internal class RestClientFactory(IMemoryCore cache, ILogger<RestClient> logger) 
    : IRestClientFactory, ISingletonService<IRestClientFactory>
{
    private static string GetKey(string? name) =>
        name ?? "__default";

    public IRestClient GetClient(string? name = null) =>
        cache.TryGetOrAdd(GetKey(name), () => new RestClient(logger), TimeSpan.FromMinutes(60))!;

    public void ResetClient(string? name = null) =>
        cache.Remove(GetKey(name));
}
