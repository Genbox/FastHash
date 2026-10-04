#if NET7_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

namespace Genbox.FastHash;

/// <summary>Provides <c>Low</c> and <c>High</c> word accessors for <see cref="UInt128" />, matching the fields of the netstandard2.1 UInt128 type.</summary>
[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "False positive: the analyzer treats a C# 14 extension block as a nested type.")]
public static class UInt128Extensions
{
    extension(UInt128 value)
    {
        /// <summary>Gets the low 64 bits of the value.</summary>
        public ulong Low => (ulong)value;

        /// <summary>Gets the high 64 bits of the value.</summary>
        public ulong High => (ulong)(value >> 64);
    }
}
#endif