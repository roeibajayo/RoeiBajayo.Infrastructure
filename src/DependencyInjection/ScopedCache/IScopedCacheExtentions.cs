using Microsoft.Extensions.DependencyInjection;

namespace RoeiBajayo.Infrastructure.DependencyInjection.ScopedCache;

public static class IScopedCacheExtentions
{
    public static void AddScopedCached(this IServiceCollection services)
    {
        services.AddScoped<IScopedCache, ScopedCacheManager>();
    }
}