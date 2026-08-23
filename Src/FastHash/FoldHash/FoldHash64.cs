using System.Runtime.CompilerServices;
using static Genbox.FastHash.FoldHash.FoldHashConstants;

namespace Genbox.FastHash.FoldHash;

/// <summary>Provides the 64-bit FoldHash algorithm.</summary>
public static class FoldHash64
{
    /// <summary>Computes the hash of a 64-bit integer using the default seed.</summary>
    /// <param name="input">The integer to hash.</param>
    /// <returns>The 64-bit FoldHash value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input) => FoldHashShared.FoldedMultiply(0x89082efa98ec4e6cUL ^ input, ARBITRARY7 ^ input);

    /// <summary>Computes the hash of a 64-bit integer.</summary>
    /// <param name="input">The integer to hash.</param>
    /// <param name="seed">The per-hasher seed.</param>
    /// <returns>The 64-bit FoldHash value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed) => ComputeIndexCore(input, seed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ComputeIndexCore(ulong input, ulong seed)
    {
        ulong accumulator = FoldHashShared.RotateRight(seed ^ ARBITRARY3, sizeof(ulong));
        ulong s0 = accumulator ^ input;
        ulong s1 = ARBITRARY7 ^ input;
        return FoldHashShared.FoldedMultiply(s0, s1);
    }

    /// <summary>Computes the hash of a byte sequence using default seeds.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 64-bit FoldHash value.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0, null);

    /// <summary>Computes the hash of a byte sequence using a per-hasher seed.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed">The per-hasher seed.</param>
    /// <returns>The 64-bit FoldHash value.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed) => ComputeHash(data, seed, null);

    /// <summary>Computes the hash of a byte sequence using shared seeds.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="sharedSeed">The shared seed array, which must contain at least six values.</param>
    /// <returns>The 64-bit FoldHash value.</returns>
    /// <exception cref="ArgumentException"><paramref name="sharedSeed"/> contains fewer than six values.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong[]? sharedSeed) => ComputeHash(data, 0, sharedSeed);

    /// <summary>Computes the hash of a byte sequence using per-hasher and shared seeds.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed">The per-hasher seed.</param>
    /// <param name="sharedSeed">The shared seed array, or <see langword="null"/> to use the default; a supplied array must contain at least six values.</param>
    /// <returns>The 64-bit FoldHash value.</returns>
    /// <exception cref="ArgumentException"><paramref name="sharedSeed"/> contains fewer than six values.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed, ulong[]? sharedSeed)
    {
        sharedSeed ??= DefaultSharedSeed;
        FoldHashShared.ValidateSharedSeed(sharedSeed, nameof(sharedSeed));

        ulong perHasherSeed = seed ^ ARBITRARY3;
        ulong accumulator = FoldHashShared.RotateRight(perHasherSeed, data.Length);

        if (data.Length <= 16)
            return FoldHashShared.HashBytesShort(data, accumulator, sharedSeed);

        return FoldHashShared.HashBytesLong(data, accumulator, sharedSeed);
    }
}