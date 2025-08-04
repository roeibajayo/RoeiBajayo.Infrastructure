using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Infrastructure.Utils.Reflection;

public static class Extensions
{
    public static bool IsDefault<T>(this T value) where T : struct =>
        value.Equals(default);

    public static bool IsEnumerable(this Type type) =>
        typeof(System.Collections.IEnumerable).IsAssignableFrom(type);

    public static bool IsEnumerableOf(this Type type, Type of) =>
        typeof(IEnumerable<>).MakeGenericType(of).IsAssignableFrom(type);
    public static bool IsEnumerableOf<T>(this Type type) =>
        IsEnumerableOf(type, typeof(T));

    public static bool IsNullable(this Type type)
    {
        if (!type.IsGenericType) return false;
        if (type.GetGenericTypeDefinition() != typeof(Nullable<>)) return false;
        return true;
    }

    public static Type? BaseType(this Type oType) =>
        oType is not null &&
        oType.IsValueType &&
        oType.IsGenericType &&
        oType.GetGenericTypeDefinition() == typeof(Nullable<>) ?
        Nullable.GetUnderlyingType(oType) :
        oType;

    public static object? GetObject(this Expression memberExpression)
    {
        return Expression.Lambda(memberExpression, null).Compile().DynamicInvoke(null);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static PropertyInfo GetPropertyInfo<T, TProperty>(this T source,
        Expression<Func<T, TProperty>> propertyLambda)
    {
        Type type = typeof(T);

        if (propertyLambda.Body is not MemberExpression member)
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a method, not a property.",
                propertyLambda.ToString()));

        var propInfo = member.Member as PropertyInfo ?? throw new ArgumentException(string.Format(
            "Expression '{0}' refers to a field, not a property.",
            propertyLambda.ToString()));

        if (type != propInfo.ReflectedType &&
            !type.IsSubclassOf(propInfo.ReflectedType!))
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a property that is not from type {1}.",
                propertyLambda.ToString(),
                type));

        return propInfo;
    }

    public static bool IsDisposed(this IDisposable obj)
    {
        return (bool)(obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(x => x.Name.Contains("disposed", StringComparison.CurrentCultureIgnoreCase) && x.FieldType == typeof(bool))?
            .GetValue(obj) ?? false);
    }

    public static bool IsAnonymous(this Type type)
    {
        var attributes = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false);
        return attributes != null && attributes.Length != 0;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static bool IsAnonymousType<T>(this T instance) =>
        IsAnonymous(typeof(T));

}
