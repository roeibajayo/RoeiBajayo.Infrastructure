using System;
using System.Linq.Expressions;

namespace Infrastructure.Utils.Expressions;

public class PropertyInvoker<T, TProperty>(string fieldName)
{
    private readonly Func<T, TProperty> _getter = PropertyInvokerHelper.GetGetValue<T, TProperty>(fieldName);
    private readonly Action<T, TProperty> _setter = PropertyInvokerHelper.GetSetValue<T, TProperty>(fieldName);

    public TProperty Get(T item) => _getter(item);
    public void Set(T item, TProperty value) => _setter(item, value);
}

file static class PropertyInvokerHelper
{
    public static Func<T, TProperty> GetGetValue<T, TProperty>(string fieldName)
    {
        var arg = Expression.Parameter(typeof(T));
        var expr = Expression.PropertyOrField(arg, fieldName);
        return Expression.Lambda<Func<T, TProperty>>(expr, arg).Compile();
    }

    public static Action<T, TProperty> GetSetValue<T, TProperty>(string fieldName)
    {
        var arg = Expression.Parameter(typeof(T));
        var expr = Expression.PropertyOrField(arg, fieldName);
        var valueExp = Expression.Parameter(typeof(TProperty));
        return Expression.Lambda<Action<T, TProperty>>(Expression.Assign(expr, valueExp), arg, valueExp).Compile();
    }

    public static Action<T, object> GetSetObjectValue<T>(string fieldName, Type fieldType)
    {
        var arg = Expression.Parameter(typeof(T));
        var expr = Expression.PropertyOrField(arg, fieldName);
        var objectParament = Expression.Parameter(typeof(object));
        var destType = Expression.Convert(objectParament, fieldType);
        return Expression.Lambda<Action<T, object>>(Expression.Assign(expr, destType), arg, objectParament).Compile();
    }

    public static Func<T, object> GetGetObjectValue<T>(string fieldName)
    {
        var arg = Expression.Parameter(typeof(T));
        var expr = Expression.PropertyOrField(arg, fieldName);
        return Expression.Lambda<Func<T, object>>(expr, arg).Compile();
    }
}
