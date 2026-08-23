namespace Genbox.FastHash.XxHash;

/// <summary>Computes 64-bit xxHash hashes from unmanaged memory.</summary>
public static class XxHash64Unsafe
{
    /// <summary>Computes a hash using a zero seed.</summary>
    /// <param name="data">A pointer to at least <paramref name="length"/> readable bytes; it may be null only when <paramref name="length"/> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return ComputeHash(data, length, 0);
    }

    /// <summary>Computes a hash for unmanaged data.</summary>
    /// <param name="data">A pointer to at least <paramref name="length"/> readable bytes; it may be null only when <paramref name="length"/> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return XxHash64.ComputeHash(new ReadOnlySpan<byte>(data, length), seed);
    }
}