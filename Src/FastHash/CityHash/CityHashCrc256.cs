#if NET8_0_OR_GREATER
namespace Genbox.FastHash.CityHash;

/// <summary>Provides hardware-accelerated 256-bit CityHash CRC functions.</summary>
public static class CityHashCrc256
{
    /// <summary>Gets whether the required CPU intrinsics are available.</summary>
    public static bool IsSupported => CityHashCrc256Unsafe.IsSupported;

    /// <summary>Computes the hash of data into a four-word result.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="result">The destination span, which must contain at least four words.</param>
    /// <exception cref="ArgumentException"><paramref name="result" /> contains fewer than four words.</exception>
    /// <exception cref="PlatformNotSupportedException">Required CPU intrinsics are unavailable.</exception>
    public static unsafe void ComputeHash(ReadOnlySpan<byte> data, Span<ulong> result)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");
        if (result.Length < 4)
            throw new ArgumentException("The result span must contain at least four ulong values.", nameof(result));

        fixed (byte* ptr = data)
        fixed (ulong* resultPtr = result)
            CityHashCrc256Unsafe.ComputeHashInternal(ptr, (uint)data.Length, resultPtr);
    }
}
#endif