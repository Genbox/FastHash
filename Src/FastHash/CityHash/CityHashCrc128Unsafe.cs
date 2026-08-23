#if NET8_0_OR_GREATER
using static Genbox.FastHash.CityHash.CityHashConstants;
using static Genbox.FastHash.CityHash.CityHashShared;

namespace Genbox.FastHash.CityHash;

/// <summary>Provides pointer-based hardware-accelerated 128-bit CityHash CRC functions.</summary>
public static class CityHashCrc128Unsafe
{
    /// <summary>Gets whether the required CPU intrinsics are available.</summary>
    public static bool IsSupported => CityHashCrc256Unsafe.IsSupported;

    /// <summary>Computes the hash of a memory region with the default seed.</summary>
    /// <param name="data">A pointer to the data to hash.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static unsafe UInt128 ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");

        return ComputeHashInternal(data, (uint)length);
    }

    /// <summary>Computes the hash of a memory region with a seed.</summary>
    /// <param name="data">A pointer to the data to hash.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative.</exception>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static unsafe UInt128 ComputeHash(byte* data, int length, UInt128 seed)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");

        return ComputeHashInternal(data, (uint)length, seed);
    }

    internal static unsafe UInt128 ComputeHashInternal(byte* data, uint length)
    {
        if (length <= 900)
            return CityHash128Unsafe.ComputeHash(data, (int)length);

        ulong* result = stackalloc ulong[4];
        CityHashCrc256Unsafe.ComputeHashInternal(data, length, result);
        return new UInt128(result[2], result[3]);
    }

    internal static unsafe UInt128 ComputeHashInternal(byte* data, uint length, UInt128 seed)
    {
        if (length <= 900)
            return CityHash128Unsafe.ComputeHash(data, (int)length, seed);

        ulong* result = stackalloc ulong[4];
        CityHashCrc256Unsafe.ComputeHashInternal(data, length, result);
        ulong u = seed.High + result[0];
        ulong v = seed.Low + result[1];
        return new UInt128(HashLen16(u, v + result[2]), HashLen16(RotateRight(v, 32), (u * K0) + result[3]));
    }
}
#endif