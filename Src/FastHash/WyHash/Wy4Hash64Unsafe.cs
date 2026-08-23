namespace Genbox.FastHash.WyHash;

/// <summary>Computes 64-bit wyhash version 4 hashes from unmanaged memory.</summary>
public static class Wy4Hash64Unsafe
{
    /// <summary>Computes a hash for unmanaged data.</summary>
    /// <param name="data">A pointer to at least <paramref name="length"/> readable bytes; it may be null only when <paramref name="length"/> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed = 0)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return Wy4Hash64.ComputeHash(new ReadOnlySpan<byte>(data, length), seed);
    }
}