using System.Runtime.CompilerServices;
using static Genbox.FastHash.AbslHash.AbslHashConstants;

namespace Genbox.FastHash.AbslHash;

internal static class AbslHashShared
{
#if NET8_0_OR_GREATER
    internal static bool IsSimdSupported => AbslHashSimd.IsSupported;
#else
    internal static bool IsSimdSupported => false;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Mix(ulong lhs, ulong rhs) => Fold128To64(lhs, rhs);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong CombineRaw(ulong state, ulong value) => Mix(state ^ value, Mul);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong CombineContiguous(ulong state, ReadOnlySpan<byte> data)
    {
        int len = data.Length;

        if (len <= 8)
            return CombineSmallContiguous(PrecombineLengthMix(state, len), data, len);

        if (len <= 16)
            return CombineContiguous9To16(PrecombineLengthMix(state, len), data, len);

        if (len <= 32)
            return CombineContiguous17To32(PrecombineLengthMix(state, len), data, len);

        return CombineLargeContiguous(state, data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong PrecombineLengthMix(ulong state, int len) => state ^ Read64(StaticRandomData, len);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong CombineSmallContiguous(ulong state, ReadOnlySpan<byte> data, int len)
    {
        ulong value;

        if (len >= 4)
            value = Read4To8(data, len);
        else if (len > 0)
            value = Read1To3(data, len);
        else
            value = 0x57;

        return CombineRaw(state, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong CombineContiguous9To16(ulong state, ReadOnlySpan<byte> data, int len) => Mix(state ^ Read64(data), Mul ^ Read64(data, len - 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong CombineContiguous17To32(ulong state, ReadOnlySpan<byte> data, int len)
    {
        ulong m0 = Mix(Read64(data) ^ 0x13198a2e03707344UL, Read64(data, 8) ^ state);
        int tail16 = len - 16;
        ulong m1 = Mix(Read64(data, tail16) ^ 0x082efa98ec4e6c89UL, Read64(data, tail16 + 8) ^ state);
        return m0 ^ m1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Read4To8(ReadOnlySpan<byte> data, int len)
    {
        ulong mostSignificant = (ulong)Read32(data) << 32;
        ulong leastSignificant = Read32(data, len - 4);
        return mostSignificant | leastSignificant;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Read1To3(ReadOnlySpan<byte> data, int len)
    {
        uint mem0 = ((uint)data[0] << 16) | data[len - 1];
        uint mem1 = (uint)data[len / 2] << 8;
        return mem0 | mem1;
    }

    private static ulong CombineLargeContiguous(ulong state, ReadOnlySpan<byte> data)
    {
#if NET8_0_OR_GREATER
        if (AbslHashSimd.IsSupported)
            return AbslHashSimd.CombineLargeContiguous(state, data);
#endif

        int len = data.Length;

        if (len <= PiecewiseChunkSize)
            return LowLevelHashLenGt32(state, data, len);

        while (len >= PiecewiseChunkSize)
        {
            state = LowLevelHashLenGt32(state, data[..PiecewiseChunkSize], PiecewiseChunkSize);
            data = data.Slice(PiecewiseChunkSize);
            len -= PiecewiseChunkSize;
        }

        return len == 0 ? state : CombineContiguous(state, data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong LowLevelHashLenGt32(ulong seed, ReadOnlySpan<byte> data, int len) => len > 64 ? LowLevelHashLenGt64(seed, data, len) : LowLevelHash33To64(seed, data, len);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Mix32Bytes(ReadOnlySpan<byte> data, int offset, ulong currentState)
    {
        ulong a = Read64(data, offset);
        ulong b = Read64(data, offset + 8);
        ulong c = Read64(data, offset + 16);
        ulong d = Read64(data, offset + 24);

        ulong cs0 = Mix(a ^ 0x13198a2e03707344UL, b ^ currentState);
        ulong cs1 = Mix(c ^ 0xa4093822299f31d0UL, d ^ currentState);
        return cs0 ^ cs1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong LowLevelHash33To64(ulong seed, ReadOnlySpan<byte> data, int len)
    {
        ulong currentState = seed ^ 0x243f6a8885a308d3UL ^ (uint)len;
        return Mix32Bytes(data, len - 32, Mix32Bytes(data, 0, currentState));
    }

    private static ulong LowLevelHashLenGt64(ulong seed, ReadOnlySpan<byte> data, int len)
    {
        ulong currentState = seed ^ 0x243f6a8885a308d3UL ^ (uint)len;
        int last32 = len - 32;
        ulong duplicatedState0 = currentState;
        ulong duplicatedState1 = currentState;
        ulong duplicatedState2 = currentState;
        int offset = 0;

        do
        {
            ulong a = Read64(data, offset);
            ulong b = Read64(data, offset + 8);
            ulong c = Read64(data, offset + 16);
            ulong d = Read64(data, offset + 24);
            ulong e = Read64(data, offset + 32);
            ulong f = Read64(data, offset + 40);
            ulong g = Read64(data, offset + 48);
            ulong h = Read64(data, offset + 56);

            currentState = Mix(a ^ 0x13198a2e03707344UL, b ^ currentState);
            duplicatedState0 = Mix(c ^ 0xa4093822299f31d0UL, d ^ duplicatedState0);
            duplicatedState1 = Mix(e ^ 0x082efa98ec4e6c89UL, f ^ duplicatedState1);
            duplicatedState2 = Mix(g ^ 0x452821e638d01377UL, h ^ duplicatedState2);

            offset += 64;
            len -= 64;
        } while (len > 64);

        currentState = (currentState ^ duplicatedState0) ^ (duplicatedState1 + duplicatedState2);

        if (len > 32)
            currentState = Mix32Bytes(data, offset, currentState);

        return Mix32Bytes(data, last32, currentState);
    }
}