using RoeiBajayo.Infrastructure.DependencyInjection.Attributes;
using RoeiBajayo.Infrastructure.DependencyInjection.Interfaces;
using RoeiBajayo.Infrastructure.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace RoeiBajayo.Infrastructure.DependencyInjection;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds all services that inherit from <see cref="ITransientService"/>, <see cref="IScopedService"/>, and <see cref="ISingletonService"/>
    /// </summary>
    /// <typeparam name="TMarker">Marker type that all services must inherit from.</typeparam>
    public static void AddDependencyInjectionServices<TMarker>(this IServiceCollection services)
    {
        var marker = typeof(TMarker);

        services.AddAllInheritsFromMarker<ITransientService>(marker, lifetime: ServiceLifetime.Transient);
        var transient = Types.GetAllInheritsFromMarker(marker, typeof(ITransientService<>));
        foreach (var type in transient)
        {
            var serviceType = type.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ITransientService<>))
                .GetGenericArguments()[0];
            var key = type.GetCustomAttribute<KeyedServiceAttribute>()?.Key;

            if (key is not null)
                services.AddKeyedTransient(serviceType, key, type);
            else
                services.AddTransient(serviceType, type);
        }

        services.AddAllInheritsFromMarker<IScopedService>(marker, lifetime: ServiceLifetime.Scoped);
        var scoped = Types.GetAllInheritsFromMarker(marker, typeof(IScopedService<>));
        foreach (var type in scoped)
        {
            var serviceType = type.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IScopedService<>))
                .GetGenericArguments()[0];
            var key = type.GetCustomAttribute<KeyedServiceAttribute>()?.Key;

            if (key is not null)
                services.AddKeyedScoped(serviceType, key, type);
            else
                services.AddScoped(serviceType, type);
        }

        services.AddAllInheritsFromMarker<ISingletonService>(marker, lifetime: ServiceLifetime.Singleton);
        var singleton = Types.GetAllInheritsFromMarker(marker, typeof(ISingletonService<>));
        foreach (var type in singleton)
        {
            var serviceType = type.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISingletonService<>))
                .GetGenericArguments()[0];
            var key = type.GetCustomAttribute<KeyedServiceAttribute>()?.Key;

            if (key is not null)
                services.AddKeyedSingleton(serviceType, key, type);
            else
                services.AddSingleton(serviceType, type);
        }
    }
}
