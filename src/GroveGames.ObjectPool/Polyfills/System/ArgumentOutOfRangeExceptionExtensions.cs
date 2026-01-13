#if !NET8_0_OR_GREATER

using System.Runtime.CompilerServices;

namespace System;

internal static class ArgumentOutOfRangeExceptionExtensions
{
    extension(ArgumentOutOfRangeException)
    {
        public static void ThrowIfNegative(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Value must be non-negative.");
            }
        }

        public static void ThrowIfNegativeOrZero(int value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
            }
        }

        public static void ThrowIfGreaterThan(int value, int other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (value > other)
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"Value must be less than or equal to {other}.");
            }
        }
    }
}

#endif
