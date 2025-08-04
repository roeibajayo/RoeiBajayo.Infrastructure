using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Utils.DependencyInjection.ScopedCache;

public static class IScopedCacheExtentions
{
    public static void AddScopedCached(this IServiceCollection services)
    {
        services.AddScoped<IScopedCache, ScopedCacheManager>();
    }
}