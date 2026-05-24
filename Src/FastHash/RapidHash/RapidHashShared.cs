using System.Runtime.CompilerServices;
using static Genbox.FastHash.RapidHash.RapidHashConstants;

namespace Genbox.FastHash.RapidHash;

internal static class RapidHashShared
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong ComputeIndex(ulong input, ulong seed)
    {
        seed ^= RapidMix(seed ^ DefaultSecret2, DefaultSecret1);
        seed ^= 8UL;
        ulong a = input ^ DefaultSecret1;
        ulong b = input ^ seed;

        ulong high = BigMul(a, b, out ulong low);
        return RapidMix(low ^ DefaultSecret7, high ^ DefaultSecret1 ^ 8UL);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong InitializeSeed(ulong seed, ulong secret1, ulong secret2) => seed ^ RapidMix(seed ^ secret2, secret1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool TryReadSmall(ReadOnlySpan<byte> data, ref ulong seed, out ulong a, out ulong b)
    {
        int length = data.Length;
        a = 0;
        b = 0;

        if (length > 16)
            return false;

        if (length >= 4)
        {
            seed ^= (ulong)length;
            if (length >= 8)
            {
                a = Read64(data);
                b = Read64(data, length - 8);
            }
            else
            {
                a = Read32(data);
                b = Read32(data, length - 4);
            }
        }
        else if (length > 0)
        {
            a = ((ulong)data[0] << 45) | data[length - 1];
            b = data[length >> 1];
        }

        return true;
    }

    internal static void MixRemainingBlocks(ReadOnlySpan<byte> data, int offset, int length, ref ulong seed, ulong secret1, ulong secret2, int maxBlocks)
    {
        if (length <= 16 || maxBlocks == 0)
            return;

        seed = RapidMix(Read64(data, offset) ^ secret2, Read64(data, offset + 8) ^ seed);
        if (length <= 32 || maxBlocks == 1)
            return;

        seed = RapidMix(Read64(data, offset + 16) ^ secret2, Read64(data, offset + 24) ^ seed);
        if (length <= 48 || maxBlocks == 2)
            return;

        seed = RapidMix(Read64(data, offset + 32) ^ secret1, Read64(data, offset + 40) ^ seed);
        if (length <= 64 || maxBlocks == 3)
            return;

        seed = RapidMix(Read64(data, offset + 48) ^ secret1, Read64(data, offset + 56) ^ seed);
        if (length <= 80 || maxBlocks == 4)
            return;

        seed = RapidMix(Read64(data, offset + 64) ^ secret2, Read64(data, offset + 72) ^ seed);
        if (length <= 96 || maxBlocks == 5)
            return;

        seed = RapidMix(Read64(data, offset + 80) ^ secret1, Read64(data, offset + 88) ^ seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ReadLongTail(ReadOnlySpan<byte> data, int offset, int length, out ulong a, out ulong b)
    {
        a = Read64(data, (offset + length) - 16) ^ (ulong)length;
        b = Read64(data, (offset + length) - 8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong FinalizeHash(int length, ulong a, ulong b, ulong seed, ulong secret1, ulong secret7)
    {
        a ^= secret1;
        b ^= seed;
        RapidMum(ref a, ref b);
        return RapidMix(a ^ secret7, b ^ secret1 ^ (ulong)length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong RapidMix(ulong a, ulong b)
    {
        ulong high = BigMul(a, b, out ulong low);
        return low ^ high;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void RapidMum(ref ulong a, ref ulong b)
    {
        ulong high = BigMul(a, b, out ulong low);
        a = low;
        b = high;
    }
}