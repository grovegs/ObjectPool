#if !NET8_0_OR_GREATER
using System.Collections.Generic;

namespace System.Collections.Frozen;

internal static class FrozenDictionary
{
    public static Dictionary<TKey, TValue> ToFrozenDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull
    {
        return ToFrozenDictionary(source, null);
    }

    public static Dictionary<TKey, TValue> ToFrozenDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey>? comparer) where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TValue>(comparer);

        foreach (var kvp in source)
        {
            dictionary.Add(kvp.Key, kvp.Value);
        }

        return dictionary;
    }
}

internal static class FrozenDictionaryExtensions
{
    public static Dictionary<TKey, TValue> ToFrozenDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector) where TKey : notnull
    {
        return ToFrozenDictionary(source, keySelector, elementSelector, null);
    }

    public static Dictionary<TKey, TValue> ToFrozenDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey>? comparer) where TKey : notnull
    {
        var dictionary = new Dictionary<TKey, TValue>(comparer);

        foreach (var item in source)
        {
            dictionary.Add(keySelector(item), elementSelector(item));
        }

        return dictionary;
    }
}
#endif
