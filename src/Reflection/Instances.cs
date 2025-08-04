using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.Reflection;

public static class Instances
{
    public static T CreateInstance<T>(Func<string, Type, object> getPropertyValueByName)
    {
        if (typeof(T).IsAnonymous())
        {
            var type = typeof(T);
            var args = type.GetProperties().Select(property => getPropertyValueByName(property.Name, property.PropertyType)).ToArray();
            return (T)Activator.CreateInstance(type, args)!;
        }
        else
        {
            var item = Activator.CreateInstance<T>()!;
            foreach (var property in item.GetType().GetProperties())
            {
                property.SetValue(item, getPropertyValueByName(property.Name, property.PropertyType));
            }
            return item;
        }
    }

    private readonly static JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    public static TTarget JsonMapTo<TTarget>(this object source)
            where TTarget : class
    {
        return (JsonMapTo(source, typeof(TTarget)) as TTarget)!;
    }
    public static object JsonMapTo(this object source, Type type)
    {
        return JsonSerializer.Deserialize(
            JsonSerializer.Serialize(source), type, options: SerializerOptions)!;
    }
    public static async Task<T> JsonMapToAsync<T>(this object source)
            where T : class
    {
        return (await JsonMapToAsync(source, typeof(T)) as T)!;
    }
    public static async Task<object> JsonMapToAsync(this object source, Type type)
    {
        using var utf8Json = new MemoryStream();
        await JsonSerializer.SerializeAsync(utf8Json, source, options: SerializerOptions);
        utf8Json.Position = 0;
        return (await JsonSerializer.DeserializeAsync(utf8Json, type, options: SerializerOptions))!;
    }

    public static T MapTo<T>(this object source) where T : class
    {
        return (source.MapTo(typeof(T)) as T)!;
    }
    public static object MapTo(this object source, Type type)
    {
        var target = Activator.CreateInstance(type)!;

        var sourceProperties = source.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .ToDictionary(x => x.Name.ToLower(), x => x);

        var targetProperties = type
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToDictionary(x => x.Name.ToLower(), x => x);

        source.FillInto(target, sourceProperties, targetProperties);

        return target;
    }
    public static IEnumerable<T> MapTo<T>(this IEnumerable<object> source) where T : class =>
        MapTo(source, typeof(T)).Cast<T>();
    public static IReadOnlyList<object> MapTo(this IEnumerable<object> source, Type type)
    {
        if (source == null || !source.Any())
            return [];

        var sourceProperties = source.First().GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .ToDictionary(x => x.Name.ToLower(), x => x);

        var targetProperties = type
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToDictionary(x => x.Name.ToLower(), x => x);

        var result = new List<object>();
        if (source is ICollection<object>)
            result.Capacity = (source as ICollection<object>)!.Count;

        foreach (var item in source)
        {
            var target = Activator.CreateInstance(type)!;
            result.Add(item.FillInto(target, sourceProperties, targetProperties));
        }

        return result;
    }

    internal static object FillInto(this object source, object target,
        Dictionary<string, FieldInfo> sourceProperties,
        Dictionary<string, FieldInfo> targetProperties)
    {
        foreach (var targetProp in targetProperties)
        {
            if (sourceProperties.TryGetValue(targetProp.Key, out FieldInfo? sourceProp))
            {
                var val = sourceProp.GetValue(source);
                if (val != null)
                {
                    if (targetProp.Value.FieldType.IsValueType || targetProp.Value.FieldType == typeof(string))
                    {
                        if (targetProp.Value.FieldType.IsNullable())
                        {
                            val = JsonMapTo(val, targetProp.Value.FieldType);
                            targetProp.Value.SetValue(target, val);
                        }
                        else
                        {
                            targetProp.Value.SetValue(target,
                                Convert.ChangeType(val, targetProp.Value.FieldType));
                        }
                    }
                    else
                    {
                        targetProp.Value.SetValue(target,
                            val.MapTo(targetProp.Value.FieldType));
                    }
                }
            }
        }

        return target;
    }

}

