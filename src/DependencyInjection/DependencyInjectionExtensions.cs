using RoeiBajayo.Infrastructure.Reflection;
using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds all types that inherit from the specified type T from the given assemblies to the service collection.
    /// </summary>
    /// <typeparam name="T">The type to find inheritors of.</typeparam>
    /// <param name="assemblies">Required assemblies to search for types.</param>
    /// <param name="addType">Whether to add the type itself as a service.</param>
    /// <param name="addImplementation">Whether to add the implementation type as a service.</param>
    /// <param name="addBaseType">Whether to add the base type(s) as a service.</param>
    /// <param name="addAllInterfaces">Whether to add all interfaces implemented by the type as services.</param>
    /// <param name="lifetime">Registered service lifetime.</param>
    /// <exception cref="ArgumentNullException">Thrown when assemblies is null.</exception>
    /// <exception cref="ArgumentException">Thrown when assemblies is empty.</exception>
    public static void AddAllInherits<T>(this IServiceCollection services,
        Assembly[] assemblies,
        bool addType = false,
        bool addImplementation = true,
        bool addBaseType = false,
        bool addAllInterfaces = false,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        if (assemblies.Length == 0)
            throw new ArgumentException("At least one assembly must be provided.", nameof(assemblies));

        var typesFromAssemblies = Types.GetAllInherits<T>(assemblies: assemblies);

        foreach (var type in typesFromAssemblies)
            AddType<T>(services, addType, addImplementation, addBaseType, addAllInterfaces, lifetime, type);
    }

    /// <summary>
    /// Adds all types that inherit from the specified type T from the assemblies that contain the type T to the service collection.
    /// </summary>
    /// <param name="addType">Whether to add the type itself as a service.</param>
    /// <param name="addImplementation">Whether to add the implementation type as a service.</param>
    /// <param name="addBaseType">Whether to add the base type(s) as a service.</param>
    /// <param name="addAllInterfaces">Whether to add all interfaces implemented by the type as services.</param>
    /// <param name="lifetime">Registered service lifetime.</param>
    /// <exception cref="ArgumentNullException">Thrown when assemblies is null.</exception>
    /// <exception cref="ArgumentException">Thrown when assemblies is empty.</exception>
    public static void AddAllInherits<T>(this IServiceCollection services,
        bool addType = false,
        bool addImplementation = true,
        bool addBaseType = false,
        bool addAllInterfaces = false,
        ServiceLifetime lifetime = ServiceLifetime.Scoped) =>
        AddAllInheritsFromMarker<T>(services, typeof(T), addType, addImplementation, addBaseType, addAllInterfaces, lifetime);

    /// <summary>
    /// Adds all types that inherit from the specified type T from the assemblies that contain the type T to the service collection.
    /// </summary>
    /// <param name="addType">Whether to add the type itself as a service.</param>
    /// <param name="addImplementation">Whether to add the implementation type as a service.</param>
    /// <param name="addBaseType">Whether to add the base type(s) as a service.</param>
    /// <param name="addAllInterfaces">Whether to add all interfaces implemented by the type as services.</param>
    /// <param name="lifetime">Registered service lifetime.</param>
    /// <exception cref="ArgumentNullException">Thrown when assemblies is null.</exception>
    /// <exception cref="ArgumentException">Thrown when assemblies is empty.</exception>
    public static void AddAllInheritsFromMarker<T>(this IServiceCollection services,
        Type marker,
        bool addType = false,
        bool addImplementation = true,
        bool addBaseType = false,
        bool addAllInterfaces = false,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var typesFromAssemblies = Types.GetAllInheritsFromMarker<T>(marker);

        foreach (var type in typesFromAssemblies)
            AddType<T>(services, addType, addImplementation, addBaseType, addAllInterfaces, lifetime, type);
    }

    private static void AddType<T>(IServiceCollection services,
        bool addType,
        bool addImplementation,
        bool addBaseType,
        bool addAllInterfaces,
        ServiceLifetime lifetime, Type type)
    {
        if (addType)
            services.Add(new ServiceDescriptor(typeof(T), type, lifetime));

        if (addImplementation)
            services.Add(new ServiceDescriptor(type, type, lifetime));

        if (addBaseType && type.BaseType is not null)
        {
            var baseType = type.BaseType;
            while (baseType is not null)
            {
                services.Add(new ServiceDescriptor(baseType, type, lifetime));
                baseType = baseType.BaseType;
            }
        }

        if (addAllInterfaces)
        {
            foreach (var i in type.GetInterfaces())
                services.Add(new ServiceDescriptor(i, type, lifetime));
        }
    }

    public static bool Remove<TService>(this IServiceCollection services, ServiceLifetime? lifetime = null)
    {
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(TService)
            && (lifetime is null || d.Lifetime == lifetime));

        if (descriptor is null)
            return false;

        services.Remove(descriptor);
        return true;
    }
    public static bool Remove<TService, TImplementation>(this IServiceCollection services, ServiceLifetime? lifetime = null)
    {
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(TService)
            && d.ImplementationType == typeof(TImplementation)
            && (lifetime is null || d.Lifetime == lifetime));

        if (descriptor is null)
            return false;

        services.Remove(descriptor);
        return true;
    }
    public static bool Remove<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory, ServiceLifetime? lifetime = null)
    {
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(TService)
            && (d.ImplementationFactory is not null && d.ImplementationFactory.Equals(implementationFactory))
            && (lifetime is null || d.Lifetime == lifetime));

        if (descriptor is null)
            return false;

        services.Remove(descriptor);
        return true;
    }
    public static int RemoveAll<TService>(this IServiceCollection services, ServiceLifetime? lifetime = null)
    {
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(TService)
            && (lifetime is null || d.Lifetime == lifetime))
            .ToArray();

        foreach (var descriptor in descriptors)
            services.Remove(descriptor);

        return descriptors.Length;
    }
    public static int RemoveAll<TService, TImplementation>(this IServiceCollection services, ServiceLifetime? lifetime = null)
    {
        var descriptors = services.Where(d =>
            d.ServiceType == typeof(TService)
            && d.ImplementationType == typeof(TImplementation)
            && (lifetime is null || d.Lifetime == lifetime))
            .ToArray();

        foreach (var descriptor in descriptors)
            services.Remove(descriptor);

        return descriptors.Length;
    }

    public static bool RemoveSingleton<TService>(this IServiceCollection services)
    {
        return services.Remove<TService>(ServiceLifetime.Singleton);
    }
    public static bool RemoveSingleton<TService, TImplementation>(this IServiceCollection services)
    {
        return services.Remove<TService, TImplementation>(ServiceLifetime.Singleton);
    }
    public static bool RemoveSingleton<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
    {
        return services.Remove<TService, TImplementation>(implementationFactory, ServiceLifetime.Singleton);
    }
    public static int RemoveAllSingleton<TService>(this IServiceCollection services)
    {
        return services.RemoveAll<TService>(ServiceLifetime.Singleton);
    }
    public static int RemoveAllSingleton<TService, TImplementation>(this IServiceCollection services)
    {
        return services.RemoveAll<TService, TImplementation>(ServiceLifetime.Singleton);
    }

    public static bool RemoveScoped<TService>(this IServiceCollection services)
    {
        return services.Remove<TService>(ServiceLifetime.Scoped);
    }
    public static bool RemoveScoped<TService, TImplementation>(this IServiceCollection services)
    {
        return services.Remove<TService, TImplementation>(ServiceLifetime.Scoped);
    }
    public static bool RemoveScoped<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
    {
        return services.Remove<TService, TImplementation>(implementationFactory, ServiceLifetime.Scoped);
    }
    public static int RemoveAllScoped<TService>(this IServiceCollection services)
    {
        return services.RemoveAll<TService>(ServiceLifetime.Scoped);
    }
    public static int RemoveAllScoped<TService, TImplementation>(this IServiceCollection services)
    {
        return services.RemoveAll<TService, TImplementation>(ServiceLifetime.Scoped);
    }

    public static bool RemoveTransient<TService>(this IServiceCollection services)
    {
        return services.Remove<TService>(ServiceLifetime.Transient);
    }
    public static bool RemoveTransient<TService, TImplementation>(this IServiceCollection services)
    {
        return services.Remove<TService, TImplementation>(ServiceLifetime.Transient);
    }
    public static bool RemoveTransient<TService, TImplementation>(this IServiceCollection services, Func<IServiceProvider, TImplementation> implementationFactory)
    {
        return services.Remove<TService, TImplementation>(implementationFactory, ServiceLifetime.Transient);
    }
    public static int RemoveAllTransient<TService>(this IServiceCollection services)
    {
        return services.RemoveAll<TService>(ServiceLifetime.Transient);
    }
    public static int RemoveAllTransient<TService, TImplementation>(this IServiceCollection services)
    {
        return services.RemoveAll<TService, TImplementation>(ServiceLifetime.Transient);

    }
}