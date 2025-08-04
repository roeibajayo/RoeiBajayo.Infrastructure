using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RoeiBajayo.Infrastructure.Reflection;

public static class Types
{
    public static IEnumerable<Type> GetAllInheritsFromMarker<T>(Type marker,
        bool ignoreAbstract = true,
        bool directBaseTypeOnly = false) =>
        GetAllInherits<T>(ignoreAbstract, directBaseTypeOnly, assemblies: marker.Assembly);

    public static IEnumerable<Type> GetAllInheritsFromMarker(Type marker,
        Type type,
        bool ignoreAbstract = true,
        bool directBaseTypeOnly = false) =>
        GetAllInherits(type, ignoreAbstract, directBaseTypeOnly, assemblies: marker.Assembly);

    public static IEnumerable<Type> GetAllInherits<T>(bool ignoreAbstract = true,
        bool directBaseTypeOnly = false,
        params Assembly[] assemblies)
    {
        var tType = typeof(T);
        return GetAllInherits(tType, ignoreAbstract, directBaseTypeOnly, assemblies);
    }

    public static IEnumerable<Type> GetAllInherits(Type type,
        bool ignoreAbstract = true,
        bool directBaseTypeOnly = false,
        params Assembly[] assemblies)
    {
        if (assemblies.Length == 0)
            assemblies = [Assembly.GetEntryAssembly()!];

        return assemblies
            .Where(x => x is not null)
            .SelectMany(x => x.DefinedTypes)
            .Where(x => (!ignoreAbstract || !x.IsAbstract) &&
                !x.IsInterface &&
                type.IsInterface ?
                    (type.IsGenericType ? x.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == type) : x.GetInterfaces().Contains(type)) :
                    directBaseTypeOnly ? x.BaseType == type : x.GetBaseTypes().Contains(type));
    }

    public static IEnumerable<Type> GetAllByAttributeFromMarker<T>(Type marker,
        bool ignoreAbstract = true,
        bool directBaseTypeOnly = false)
        where T : Attribute
    {
        return GetAllByAttribute<T>(ignoreAbstract, directBaseTypeOnly, assemblies: marker.Assembly);
    }
    public static IEnumerable<Type> GetAllByAttribute<T>(bool ignoreAbstract = true,
        bool classOnly = true,
        params Assembly[] assemblies)
        where T : Attribute
    {
        if (assemblies.Length == 0)
            assemblies = [Assembly.GetEntryAssembly()!];

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.DefinedTypes)
            {
                if ((classOnly && !type.IsClass) || (ignoreAbstract && type.IsAbstract))
                    continue;

                if (type.GetCustomAttribute<T>() is not null)
                    yield return type;
            }
        }
    }

    public static IEnumerable<Type> GetBaseTypes<T>() =>
        GetBaseTypes(typeof(T));
    public static IEnumerable<Type> GetBaseTypes(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        while (type.BaseType is not null)
        {
            yield return type.BaseType;
            type = type.BaseType;
        }
    }
}
