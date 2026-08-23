namespace Genbox.FastHash.CityHash;

/// <summary>Provides pointer-based 64-bit CityHash functions.</summary>
public static class CityHash64Unsafe
{
    /// <summary>Computes the hash of a memory region with the default seed.</summary>
    /// <param name="data">A pointer to the data to hash.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return CityHash64.ComputeHash(new ReadOnlySpan<byte>(data, length));
    }

    /// <summary>Computes the hash of a memory region with a seed.</summary>
    /// <param name="data">A pointer to the data to hash.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return CityHash64.ComputeHash(new ReadOnlySpan<byte>(data, length), seed);
    }

    /// <summary>Computes the hash of a memory region with two seeds.</summary>
    /// <param name="data">A pointer to the data to hash.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed1">The first hash seed.</param>
    /// <param name="seed2">The second hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed1, ulong seed2)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return CityHash64.ComputeHash(new ReadOnlySpan<byte>(data, length), seed1, seed2);
    }
}