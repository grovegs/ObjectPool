using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace GroveGames.ObjectPool;

internal static class ThrowHelper
{
    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowNegativeValue(string paramName)
    {
        throw new ArgumentOutOfRangeException(paramName, "Value must be non-negative.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowGreaterThan(string paramName, int value, int max)
    {
        throw new ArgumentOutOfRangeException(paramName, value, $"Value must be less than or equal to {max}.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowNegativeOrZero(string paramName)
    {
        throw new ArgumentOutOfRangeException(paramName, "Value must be positive.");
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowObjectDisposed(object instance)
    {
        throw new ObjectDisposedException(instance.GetType().FullName);
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void ThrowArgumentNull(string paramName)
    {
        throw new ArgumentNullException(paramName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNegative(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
        {
            ThrowNegativeValue(paramName!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfGreaterThan(int value, int max, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value > max)
        {
            ThrowGreaterThan(paramName!, value, max);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNegativeOrZero(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value <= 0)
        {
            ThrowNegativeOrZero(paramName!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfDisposed(bool disposed, object instance)
    {
        if (disposed)
        {
            ThrowObjectDisposed(instance);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfNull<T>(T? value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : class
    {
        if (value is null)
        {
            ThrowArgumentNull(paramName!);
        }
    }
}