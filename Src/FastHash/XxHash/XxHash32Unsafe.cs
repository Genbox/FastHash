// C# port of Cyan4973/xxHash 0.8.3 (e626a72). Copyright (c) 2012-2021 Yann Collet.
// Distributed under the BSD 2-Clause license; see THIRD-PARTY-NOTICES.txt at the repository root.
namespace Genbox.FastHash.XxHash;

/// <summary>Computes 32-bit xxHash hashes from unmanaged memory.</summary>
public static class XxHash32Unsafe
{
    /// <summary>Computes a hash using a zero seed.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> readable bytes; it may be null only when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 32-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe uint ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return ComputeHash(data, length, 0);
    }

    /// <summary>Computes a hash for unmanaged data.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> readable bytes; it may be null only when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 32-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe uint ComputeHash(byte* data, int length, uint seed)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return XxHash32.ComputeHash(new ReadOnlySpan<byte>(data, length), seed);
    }
}