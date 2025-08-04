using System;

namespace Microsoft.Extensions.DependencyInjection;

public static class LazyServices
{
    internal class LazyFactory<T>(IServiceProvider provider) : Lazy<T>(provider.GetRequiredService<T>) where T : class
    {
    }

    /// <summary>
    /// Adds support for lazy loading of services in the dependency injection container, so that you can inject a <see cref="Lazy{T}"/> instance instead of the service itself.
    /// </summary>
    public static void AddLazySupport(this IServiceCollection services)
    {
        services.AddTransient(typeof(Lazy<>), typeof(LazyFactory<>));
    }
}