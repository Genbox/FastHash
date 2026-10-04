// C# port of wangyi-fudan/wyhash 4.3.0 (2ac9a50) by Wang Yi.
// Distributed under the Unlicense; see THIRD-PARTY-NOTICES.txt at the repository root.
namespace Genbox.FastHash.WyHash;

/// <summary>Computes 64-bit wyhash final version 4.3 hashes from unmanaged memory.</summary>
/// <remarks>Define <c>WYHASH_CONDOM</c> at build time to select upstream mode 2 (blind multiplication); otherwise upstream mode 1 is used.</remarks>
public static class Wy4Hash64Unsafe
{
    /// <summary>Computes a hash for unmanaged data.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> readable bytes; it may be null only when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed = 0)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return Wy4Hash64.ComputeHash(new ReadOnlySpan<byte>(data, length), seed);
    }
}