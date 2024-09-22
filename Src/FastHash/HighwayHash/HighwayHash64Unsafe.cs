using System.Runtime.CompilerServices;

namespace Genbox.FastHash.HighwayHash;

public static class HighwayHash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int size, ulong seed1, ulong seed2, ulong seed3, ulong seed4)
    {
        uint len = (uint)size;

        ulong[] key = [seed1, seed2, seed3, seed4];

        HighwayHashState state = new HighwayHashState();
        ProcessAll(data, len, key, ref state);
        return HighwayHashFinalize64(state);
    }

    public static unsafe ulong ComputeHash(byte* data, int size)
    {
        uint len = (uint)size;

        HighwayHashState state = new HighwayHashState();
        ProcessAll(data, len, HighwayHashConstants.DefaultKeys, ref state);
        return HighwayHashFinalize64(state);
    }

    public static unsafe ulong ComputeHash(byte* data, int size, ulong[] keys)
    {
        uint len = (uint)size;

        HighwayHashState state = new HighwayHashState();
        ProcessAll(data, len, keys, ref state);
        return HighwayHashFinalize64(state);
    }

    private static void HighwayHashReset(ulong[] key, ref HighwayHashState state)
    {
        state.mul0_0 = 0xdbe6d5d5fe4cce2ful;
        state.mul0_1 = 0xa4093822299f31d0ul;
        state.mul0_2 = 0x13198a2e03707344ul;
        state.mul0_3 = 0x243f6a8885a308d3ul;
        state.mul1_0 = 0x3bd39e10cb0ef593ul;
        state.mul1_1 = 0xc0acf169b5f18a8cul;
        state.mul1_2 = 0xbe5466cf34e90c6cul;
        state.mul1_3 = 0x452821e638d01377ul;
        state.v0_0 = state.mul0_0 ^ key[0];
        state.v0_1 = state.mul0_1 ^ key[1];
        state.v0_2 = state.mul0_2 ^ key[2];
        state.v0_3 = state.mul0_3 ^ key[3];
        state.v1_0 = state.mul1_0 ^ ((key[0] >> 32) | (key[0] << 32));
        state.v1_1 = state.mul1_1 ^ ((key[1] >> 32) | (key[1] << 32));
        state.v1_2 = state.mul1_2 ^ ((key[2] >> 32) | (key[2] << 32));
        state.v1_3 = state.mul1_3 ^ ((key[3] >> 32) | (key[3] << 32));
    }

    private static unsafe void ProcessAll(byte* data, uint size, ulong[] key, ref HighwayHashState state)
    {
        uint i;
        HighwayHashReset(key, ref state);

        for (i = 0; i + 32 <= size; i += 32)
            HighwayHashUpdatePacket(data + i, ref state);

        if ((size & 31) != 0)
            HighwayHashUpdateRemainder(data + i, size & 31, ref state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void HighwayHashUpdatePacket(byte* packet, ref HighwayHashState state)
    {
        ulong* lanes = stackalloc ulong[4];
        lanes[0] = Read64(packet + 0);
        lanes[1] = Read64(packet + 8);
        lanes[2] = Read64(packet + 16);
        lanes[3] = Read64(packet + 24);
        Update(lanes, ref state);
    }

    private static unsafe void Update(ulong* lanes, ref HighwayHashState state)
    {
        state.v1_0 += state.mul0_0 + lanes[0];
        state.mul0_0 ^= (state.v1_0 & 0xffffffff) * (state.v0_0 >> 32);
        state.v0_0 += state.mul1_0;
        state.mul1_0 ^= (state.v0_0 & 0xffffffff) * (state.v1_0 >> 32);

        state.v1_1 += state.mul0_1 + lanes[1];
        state.mul0_1 ^= (state.v1_1 & 0xffffffff) * (state.v0_1 >> 32);
        state.v0_1 += state.mul1_1;
        state.mul1_1 ^= (state.v0_1 & 0xffffffff) * (state.v1_1 >> 32);

        state.v1_2 += state.mul0_2 + lanes[2];
        state.mul0_2 ^= (state.v1_2 & 0xffffffff) * (state.v0_2 >> 32);
        state.v0_2 += state.mul1_2;
        state.mul1_2 ^= (state.v0_2 & 0xffffffff) * (state.v1_2 >> 32);

        state.v1_3 += state.mul0_3 + lanes[3];
        state.mul0_3 ^= (state.v1_3 & 0xffffffff) * (state.v0_3 >> 32);
        state.v0_3 += state.mul1_3;
        state.mul1_3 ^= (state.v0_3 & 0xffffffff) * (state.v1_3 >> 32);

        ZipperMergeAndAdd(state.v1_1, state.v1_0, ref state.v0_1, ref state.v0_0);
        ZipperMergeAndAdd(state.v1_3, state.v1_2, ref state.v0_3, ref state.v0_2);
        ZipperMergeAndAdd(state.v0_1, state.v0_0, ref state.v1_1, ref state.v1_0);
        ZipperMergeAndAdd(state.v0_3, state.v0_2, ref state.v1_3, ref state.v1_2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ZipperMergeAndAdd(ulong v1, ulong v0, ref ulong add1, ref ulong add0)
    {
        add0 += (((v0 & 0xff000000ul) | (v1 & 0xff00000000ul)) >> 24) |
                (((v0 & 0xff0000000000ul) | (v1 & 0xff000000000000ul)) >> 16) |
                (v0 & 0xff0000ul) | ((v0 & 0xff00ul) << 32) |
                ((v1 & 0xff00000000000000ul) >> 8) | (v0 << 56);

        add1 += (((v1 & 0xff000000ul) | (v0 & 0xff00000000ul)) >> 24) |
                (v1 & 0xff0000ul) | ((v1 & 0xff0000000000ul) >> 16) |
                ((v1 & 0xff00ul) << 24) | ((v0 & 0xff000000000000ul) >> 8) |
                ((v1 & 0xfful) << 48) | (v0 & 0xff00000000000000ul);
    }

    private static unsafe void HighwayHashUpdateRemainder(byte* bytes, uint size_mod32, ref HighwayHashState state)
    {
        int i;
        uint size_mod4 = size_mod32 & 3;
        byte* remainder = bytes + (size_mod32 & ~3);
        byte* packet = stackalloc byte[32];

        state.v0_0 += ((ulong)size_mod32 << 32) + size_mod32;
        state.v0_1 += ((ulong)size_mod32 << 32) + size_mod32;
        state.v0_2 += ((ulong)size_mod32 << 32) + size_mod32;
        state.v0_3 += ((ulong)size_mod32 << 32) + size_mod32;

        int count = (int)size_mod32;

        uint half0 = (uint)(state.v1_0 & 0xffffffff);
        uint half1 = (uint)(state.v1_0 >> 32);
        state.v1_0 = (half0 << count) | (half0 >> (32 - count));
        state.v1_0 |= (ulong)((half1 << count) | (half1 >> (32 - count))) << 32;

        half0 = (uint)(state.v1_1 & 0xffffffff);
        half1 = (uint)(state.v1_1 >> 32);
        state.v1_1 = (half0 << count) | (half0 >> (32 - count));
        state.v1_1 |= (ulong)((half1 << count) | (half1 >> (32 - count))) << 32;

        half0 = (uint)(state.v1_2 & 0xffffffff);
        half1 = (uint)(state.v1_2 >> 32);
        state.v1_2 = (half0 << count) | (half0 >> (32 - count));
        state.v1_2 |= (ulong)((half1 << count) | (half1 >> (32 - count))) << 32;

        half0 = (uint)(state.v1_3 & 0xffffffff);
        half1 = (uint)(state.v1_3 >> 32);
        state.v1_3 = (half0 << count) | (half0 >> (32 - count));
        state.v1_3 |= (ulong)((half1 << count) | (half1 >> (32 - count))) << 32;

        for (i = 0; i < remainder - bytes; i++)
            packet[i] = bytes[i];

        if ((size_mod32 & 16) != 0)
        {
            for (i = 0; i < 4; i++)
                packet[28 + i] = remainder[i + size_mod4 - 4];
        }
        else
        {
            if (size_mod4 != 0)
            {
                packet[16 + 0] = remainder[0];
                packet[16 + 1] = remainder[size_mod4 >> 1];
                packet[16 + 2] = remainder[size_mod4 - 1];
            }
        }
        HighwayHashUpdatePacket(packet, ref state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong HighwayHashFinalize64(HighwayHashState state)
    {
        for (int i = 0; i < 4; i++)
            PermuteAndUpdate(ref state);

        return state.v0_0 + state.v1_0 + state.mul0_0 + state.mul1_0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void PermuteAndUpdate(ref HighwayHashState state)
    {
        ulong* permuted = stackalloc ulong[4];

        // Permute(state.v0, permuted);
        permuted[0] = (state.v0_2 >> 32) | (state.v0_2 << 32);
        permuted[1] = (state.v0_3 >> 32) | (state.v0_3 << 32);
        permuted[2] = (state.v0_0 >> 32) | (state.v0_0 << 32);
        permuted[3] = (state.v0_1 >> 32) | (state.v0_1 << 32);

        Update(permuted, ref state);
    }
}