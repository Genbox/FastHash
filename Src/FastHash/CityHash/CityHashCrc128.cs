#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;

namespace Genbox.FastHash.CityHash;

public static class CityHashCrc128
{
    public static bool IsSupported => CityHashCrc128Unsafe.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");

        return CityHash128.ComputeIndex(input);
    }

    public static unsafe UInt128 ComputeHash(ReadOnlySpan<byte> data)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");

        fixed (byte* ptr = data)
            return CityHashCrc128Unsafe.ComputeHashInternal(ptr, (uint)data.Length);
    }

    public static unsafe UInt128 ComputeHash(ReadOnlySpan<byte> data, UInt128 seed)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("CityHashCrc requires SSE4.2 x64 intrinsics.");

        fixed (byte* ptr = data)
            return CityHashCrc128Unsafe.ComputeHashInternal(ptr, (uint)data.Length, seed);
    }
}
#endif