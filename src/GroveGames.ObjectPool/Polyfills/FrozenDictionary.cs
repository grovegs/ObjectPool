#if !NET8_0_OR_GREATER
using System.Collections.Generic;
using System.Collections.Immutable;

namespace System.Collections.Frozen;

internal static class FrozenDictionary
{
    public static ImmutableDictionary<TKey, TValue> ToFrozenDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source) where TKey : notnull
    {
        var builder = ImmutableDictionary.CreateBuilder<TKey, TValue>();

        foreach (var kvp in source)
        {
            builder.Add(kvp.Key, kvp.Value);
        }

        return builder.ToImmutable();
    }

    public static ImmutableDictionary<TKey, TValue> ToFrozenDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source, IEqualityComparer<TKey>? comparer) where TKey : notnull
    {
        var builder = ImmutableDictionary.CreateBuilder<TKey, TValue>(comparer);

        foreach (var kvp in source)
        {
            builder.Add(kvp.Key, kvp.Value);
        }

        return builder.ToImmutable();
    }
}

internal static class FrozenDictionaryExtensions
{
    public static ImmutableDictionary<TKey, TValue> ToFrozenDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector) where TKey : notnull
    {
        var builder = ImmutableDictionary.CreateBuilder<TKey, TValue>();

        foreach (var item in source)
        {
            builder.Add(keySelector(item), elementSelector(item));
        }

        return builder.ToImmutable();
    }

    public static ImmutableDictionary<TKey, TValue> ToFrozenDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> elementSelector, IEqualityComparer<TKey>? comparer) where TKey : notnull
    {
        var builder = ImmutableDictionary.CreateBuilder<TKey, TValue>(comparer);

        foreach (var item in source)
        {
            builder.Add(keySelector(item), elementSelector(item));
        }

        return builder.ToImmutable();
    }
}
#endif