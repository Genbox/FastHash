#if NET10_0_OR_GREATER

// C# port of orlp/polyxor 0.1.0 (3123eb6). Copyright (c) 2026 Orson Peters.
// Distributed under the zlib license; see LICENSE.txt in this directory.
using System.Runtime.CompilerServices;

namespace Genbox.FastHash.PolyXorHash;

/// <summary>Provides pointer-based PolyXOR128 hashing using the same accelerated implementation.</summary>
public static unsafe class PolyXorHash128Unsafe
{
    /// <summary>Gets whether hardware carryless multiplication is available.</summary>
    public static bool IsSupported => PolyXorHash128.IsSupported;

    /// <summary>Computes an avalanched hash using the fixed all-zero key.</summary>
    /// <param name="data">The input pointer.</param>
    /// <param name="length">The number of readable bytes.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeHash(byte* data, int length) => PolyXorHash128.ComputeHash(new ReadOnlySpan<byte>(data, length));

    /// <summary>Computes an avalanched hash using parameters derived from a key.</summary>
    /// <param name="data">The input pointer.</param>
    /// <param name="length">The number of readable bytes.</param>
    /// <param name="key">The 128-bit AES key used to derive the parameters.</param>
    /// <param name="tweak">The avalanche tweak.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <remarks>Expands the key on each call. Reuse <see cref="PolyXorHashParams" /> when hashing multiple messages.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">AES key expansion failed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeHash(byte* data, int length, UInt128 key, ulong tweak = 0) =>
        PolyXorHash128.ComputeHash(new ReadOnlySpan<byte>(data, length), key, tweak);

    /// <summary>Computes an avalanched hash using reusable parameters.</summary>
    /// <param name="data">The input pointer.</param>
    /// <param name="length">The number of readable bytes.</param>
    /// <param name="parameters">The expanded parameters.</param>
    /// <param name="tweak">The avalanche tweak.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeHash(byte* data, int length, PolyXorHashParams parameters, ulong tweak = 0) =>
        PolyXorHash128.ComputeHash(new ReadOnlySpan<byte>(data, length), parameters, tweak);
}
#endif