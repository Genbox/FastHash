#if NET8_0_OR_GREATER
namespace Genbox.FastHash.CityHash;

public static class CityHashCrc256
{
    public static bool IsSupported => CityHashCrc256Unsafe.IsSupported;

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