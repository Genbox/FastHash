#if NET10_0_OR_GREATER

// C# port of orlp/polyxor 0.1.0 (3123eb6). Copyright (c) 2026 Orson Peters.
// Distributed under the zlib license; see LICENSE.txt in this directory.
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Genbox.FastHash.PolyXorHash;

/// <summary>Provides hardware-accelerated PolyXOR128 hashing.</summary>
/// <remarks>Raw and avalanched hashes do not hide the key. Use MAC output when authentication is required.</remarks>
public static class PolyXorHash128
{
    /// <summary>Gets whether hardware carryless multiplication is available.</summary>
    public static bool IsSupported => PolyXorHashShared.IsSupported;

    private static class DefaultParameters
    {
        internal static readonly PolyXorHashParams Value = new PolyXorHashParams(default(UInt128));
    }

    /// <summary>Computes an avalanched hash using the fixed all-zero key.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, DefaultParameters.Value);

    /// <summary>Computes an avalanched hash using parameters derived from a key.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="key">The 128-bit AES key used to derive the parameters.</param>
    /// <param name="tweak">An optional value mixed in before avalanching.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <remarks>Expands the key on each call. Reuse <see cref="PolyXorHashParams" /> when hashing multiple messages.</remarks>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">AES key expansion failed.</exception>
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, UInt128 key, ulong tweak = 0) =>
        ComputeHash(data, new PolyXorHashParams(key), tweak);

    /// <summary>Computes an avalanched hash using reusable parameters.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="parameters">The expanded key or entropy.</param>
    /// <param name="tweak">An optional value mixed in before avalanching.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, PolyXorHashParams parameters, ulong tweak = 0) =>
        PolyXorHashShared.ToUInt128(PolyXorHashShared.Avalanche(ComputeRaw(data, parameters), parameters, tweak));

    /// <summary>Computes the raw universal hash without avalanching or blinding.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="parameters">The expanded key or entropy.</param>
    /// <returns>The raw 128-bit hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    public static UInt128 ComputeHashRaw(ReadOnlySpan<byte> data, PolyXorHashParams parameters) =>
        PolyXorHashShared.ToUInt128(ComputeRaw(data, parameters));

    /// <summary>Computes the raw universal hash using parameters derived from a key.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="key">The 128-bit AES key used to derive the parameters.</param>
    /// <returns>The raw 128-bit hash.</returns>
    /// <remarks>Expands the key on each call. Reuse <see cref="PolyXorHashParams" /> when hashing multiple messages.</remarks>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">AES key expansion failed.</exception>
    public static UInt128 ComputeHashRaw(ReadOnlySpan<byte> data, UInt128 key) => ComputeHashRaw(data, new PolyXorHashParams(key));

    /// <summary>Computes a nonce-based authentication tag.</summary>
    /// <param name="data">The bytes to authenticate.</param>
    /// <param name="parameters">Parameters expanded from an AES key.</param>
    /// <param name="nonce">The nonce; the top two bits are ignored.</param>
    /// <returns>The 128-bit tag.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="parameters" /> was initialized from entropy instead of an AES key.</exception>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication or AES intrinsics are unavailable.</exception>
    public static UInt128 ComputeMac(ReadOnlySpan<byte> data, PolyXorHashParams parameters, UInt128 nonce)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        return parameters.Mac(ComputeRaw(data, parameters), nonce);
    }

    /// <summary>Computes a nonce-based authentication tag using parameters derived from a key.</summary>
    /// <param name="data">The bytes to authenticate.</param>
    /// <param name="key">The 128-bit AES key.</param>
    /// <param name="nonce">The nonce; the top two bits are ignored.</param>
    /// <returns>The 128-bit tag.</returns>
    /// <remarks>Expands the key on each call. Reuse <see cref="PolyXorHashParams" /> when authenticating multiple messages.</remarks>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication or AES intrinsics are unavailable.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">AES key expansion failed.</exception>
    public static UInt128 ComputeMac(ReadOnlySpan<byte> data, UInt128 key, UInt128 nonce) => ComputeMac(data, new PolyXorHashParams(key), nonce);

    /// <summary>Hashes a 64-bit integer using the fixed all-zero key.</summary>
    /// <param name="input">The integer encoded in little-endian order.</param>
    /// <returns>The avalanched hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input) => ComputeIndex(input, DefaultParameters.Value);

    /// <summary>Hashes a 64-bit integer using parameters derived from a key.</summary>
    /// <param name="input">The integer encoded in little-endian order.</param>
    /// <param name="key">The 128-bit AES key used to derive the parameters.</param>
    /// <param name="tweak">The avalanche tweak.</param>
    /// <returns>The avalanched hash.</returns>
    /// <remarks>Expands the key on each call. Reuse <see cref="PolyXorHashParams" /> when hashing multiple integers.</remarks>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">AES key expansion failed.</exception>
    public static UInt128 ComputeIndex(ulong input, UInt128 key, ulong tweak = 0) => ComputeIndex(input, new PolyXorHashParams(key), tweak);

    /// <summary>Hashes a 64-bit integer using reusable parameters.</summary>
    /// <param name="input">The integer encoded in little-endian order.</param>
    /// <param name="parameters">The expanded parameters.</param>
    /// <param name="tweak">The avalanche tweak.</param>
    /// <returns>The avalanched hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input, PolyXorHashParams parameters, ulong tweak = 0)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        PolyXorHashShared.RequireSupported();
        Vector128<ulong> raw = PolyXorHashShared.HashIndexRaw(input, parameters);
        return PolyXorHashShared.ToUInt128(PolyXorHashShared.Avalanche(raw, parameters, tweak));
    }

    private static Vector128<ulong> ComputeRaw(ReadOnlySpan<byte> data, PolyXorHashParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        PolyXorHashShared.RequireSupported();
        if (data.Length <= sizeof(ulong))
            return PolyXorHashShared.HashShortRaw(data, parameters);
        Vector128<ulong> accum = parameters.PolyZ;
        int whole = data.Length & ~4095;
        if (whole != 0)
            PolyXorHashShared.HashBlocks(data[..whole], parameters, ref accum);
        if (whole != data.Length)
            PolyXorHashShared.HashTail(data[whole..], parameters, ref accum);
        return PolyXorHashShared.Multiply(accum ^ parameters.PolyU, Vector128.Create((ulong)data.Length, 0UL) ^ parameters.PolyY);
    }
}
#endif