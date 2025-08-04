using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RoeiBajayo.Infrastructure.IEnumerable;

public static class IDictionaryExtentions
{
    public static bool TrySetValueNonThreadSafe<TKey, TValue>(this Dictionary<TKey, TValue> dictionary,
        TKey key, TValue value) where TKey : notnull
    {
        ref var valOrNull = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);
        if (!Unsafe.IsNullRef(ref valOrNull))
        {
            valOrNull = value;
            return true;
        }
        return false;
    }
}
