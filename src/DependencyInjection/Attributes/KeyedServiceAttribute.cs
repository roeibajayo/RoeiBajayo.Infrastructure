using System;

namespace Infrastructure.Utils.DependencyInjection.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class KeyedServiceAttribute(object key) : Attribute
{
    public object Key { get; } = key;
}