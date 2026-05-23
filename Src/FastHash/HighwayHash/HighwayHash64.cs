using System.Runtime.CompilerServices;

namespace Genbox.FastHash.HighwayHash;

public static class HighwayHash64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        HighwayHashState state = new HighwayHashState();
        Reset(HighwayHashConstants.DefaultKey0, HighwayHashConstants.DefaultKey1, HighwayHashConstants.DefaultKey2, HighwayHashConstants.DefaultKey3, ref state);
        UpdateIndex(input, ref state);
        return Finalize64(state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed1, ulong seed2, ulong seed3, ulong seed4)
    {
        HighwayHashState state = new HighwayHashState();
        Reset(seed1, seed2, seed3, seed4, ref state);
        UpdateIndex(input, ref state);
        return Finalize64(state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong[] keys)
    {
        HighwayHashState state = new HighwayHashState();
        Reset(keys, ref state);
        UpdateIndex(input, ref state);
        return Finalize64(state);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data)
    {
        HighwayHashState state = new HighwayHashState();
        ProcessAll(data, HighwayHashConstants.DefaultKey0, HighwayHashConstants.DefaultKey1, HighwayHashConstants.DefaultKey2, HighwayHashConstants.DefaultKey3, ref state);
        return Finalize64(state);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed1, ulong seed2, ulong seed3, ulong seed4)
    {
        HighwayHashState state = new HighwayHashState();
        ProcessAll(data, seed1, seed2, seed3, seed4, ref state);
        return Finalize64(state);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong[] keys)
    {
        HighwayHashState state = new HighwayHashState();
        ProcessAll(data, keys, ref state);
        return Finalize64(state);
    }

    private static void Reset(ulong[] key, ref HighwayHashState state)
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

    private static void Reset(ulong key0, ulong key1, ulong key2, ulong key3, ref HighwayHashState state)
    {
        state.mul0_0 = 0xdbe6d5d5fe4cce2ful;
        state.mul0_1 = 0xa4093822299f31d0ul;
        state.mul0_2 = 0x13198a2e03707344ul;
        state.mul0_3 = 0x243f6a8885a308d3ul;
        state.mul1_0 = 0x3bd39e10cb0ef593ul;
        state.mul1_1 = 0xc0acf169b5f18a8cul;
        state.mul1_2 = 0xbe5466cf34e90c6cul;
        state.mul1_3 = 0x452821e638d01377ul;
        state.v0_0 = state.mul0_0 ^ key0;
        state.v0_1 = state.mul0_1 ^ key1;
        state.v0_2 = state.mul0_2 ^ key2;
        state.v0_3 = state.mul0_3 ^ key3;
        state.v1_0 = state.mul1_0 ^ ((key0 >> 32) | (key0 << 32));
        state.v1_1 = state.mul1_1 ^ ((key1 >> 32) | (key1 << 32));
        state.v1_2 = state.mul1_2 ^ ((key2 >> 32) | (key2 << 32));
        state.v1_3 = state.mul1_3 ^ ((key3 >> 32) | (key3 << 32));
    }

    private static void ProcessAll(ReadOnlySpan<byte> data, ulong[] key, ref HighwayHashState state)
    {
        int size = data.Length;
        Reset(key, ref state);

        int i;
        for (i = 0; i + 32 <= size; i += 32)
            UpdatePacket(data, i, ref state);

        int remainder = size & 31;
        if (remainder != 0)
            UpdateRemainder(data.Slice(i, remainder), remainder, ref state);
    }

    private static void ProcessAll(ReadOnlySpan<byte> data, ulong key0, ulong key1, ulong key2, ulong key3, ref HighwayHashState state)
    {
        int size = data.Length;
        Reset(key0, key1, key2, key3, ref state);

        int i;
        for (i = 0; i + 32 <= size; i += 32)
            UpdatePacket(data, i, ref state);

        int remainder = size & 31;
        if (remainder != 0)
            UpdateRemainder(data.Slice(i, remainder), remainder, ref state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdatePacket(ReadOnlySpan<byte> packet, int offset, ref HighwayHashState state)
    {
        Update(Read64(packet, offset), Read64(packet, offset + 8), Read64(packet, offset + 16), Read64(packet, offset + 24), ref state);
    }

    private static void Update(ulong lane0, ulong lane1, ulong lane2, ulong lane3, ref HighwayHashState state)
    {
        state.v1_0 += state.mul0_0 + lane0;
        state.mul0_0 ^= (state.v1_0 & 0xffffffff) * (state.v0_0 >> 32);
        state.v0_0 += state.mul1_0;
        state.mul1_0 ^= (state.v0_0 & 0xffffffff) * (state.v1_0 >> 32);

        state.v1_1 += state.mul0_1 + lane1;
        state.mul0_1 ^= (state.v1_1 & 0xffffffff) * (state.v0_1 >> 32);
        state.v0_1 += state.mul1_1;
        state.mul1_1 ^= (state.v0_1 & 0xffffffff) * (state.v1_1 >> 32);

        state.v1_2 += state.mul0_2 + lane2;
        state.mul0_2 ^= (state.v1_2 & 0xffffffff) * (state.v0_2 >> 32);
        state.v0_2 += state.mul1_2;
        state.mul1_2 ^= (state.v0_2 & 0xffffffff) * (state.v1_2 >> 32);

        state.v1_3 += state.mul0_3 + lane3;
        state.mul0_3 ^= (state.v1_3 & 0xffffffff) * (state.v0_3 >> 32);
        state.v0_3 += state.mul1_3;
        state.mul1_3 ^= (state.v0_3 & 0xffffffff) * (state.v1_3 >> 32);

        ZipperMergeAndAdd(state.v1_1, state.v1_0, ref state.v0_1, ref state.v0_0);
        ZipperMergeAndAdd(state.v1_3, state.v1_2, ref state.v0_3, ref state.v0_2);
        ZipperMergeAndAdd(state.v0_1, state.v0_0, ref state.v1_1, ref state.v1_0);
        ZipperMergeAndAdd(state.v0_3, state.v0_2, ref state.v1_3, ref state.v1_2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdateIndex(ulong input, ref HighwayHashState state)
    {
        const ulong length = 0x0000000800000008UL;
        state.v0_0 += length;
        state.v0_1 += length;
        state.v0_2 += length;
        state.v0_3 += length;

        state.v1_0 = RotateHalvesLeft8(state.v1_0);
        state.v1_1 = RotateHalvesLeft8(state.v1_1);
        state.v1_2 = RotateHalvesLeft8(state.v1_2);
        state.v1_3 = RotateHalvesLeft8(state.v1_3);

        Update(input, 0, 0, 0, ref state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong RotateHalvesLeft8(ulong value)
    {
        uint low = RotateLeft((uint)value, 8);
        uint high = RotateLeft((uint)(value >> 32), 8);
        return low | ((ulong)high << 32);
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

    private static void UpdateRemainder(ReadOnlySpan<byte> bytes, int sizeMod32, ref HighwayHashState state)
    {
        int sizeMod4 = sizeMod32 & 3;
        int remainderOffset = sizeMod32 & ~3;
        Span<byte> packet = stackalloc byte[32];

        state.v0_0 += ((ulong)sizeMod32 << 32) + (uint)sizeMod32;
        state.v0_1 += ((ulong)sizeMod32 << 32) + (uint)sizeMod32;
        state.v0_2 += ((ulong)sizeMod32 << 32) + (uint)sizeMod32;
        state.v0_3 += ((ulong)sizeMod32 << 32) + (uint)sizeMod32;

        uint half0 = (uint)(state.v1_0 & 0xffffffff);
        uint half1 = (uint)(state.v1_0 >> 32);
        state.v1_0 = (half0 << sizeMod32) | (half0 >> (32 - sizeMod32));
        state.v1_0 |= (ulong)((half1 << sizeMod32) | (half1 >> (32 - sizeMod32))) << 32;

        half0 = (uint)(state.v1_1 & 0xffffffff);
        half1 = (uint)(state.v1_1 >> 32);
        state.v1_1 = (half0 << sizeMod32) | (half0 >> (32 - sizeMod32));
        state.v1_1 |= (ulong)((half1 << sizeMod32) | (half1 >> (32 - sizeMod32))) << 32;

        half0 = (uint)(state.v1_2 & 0xffffffff);
        half1 = (uint)(state.v1_2 >> 32);
        state.v1_2 = (half0 << sizeMod32) | (half0 >> (32 - sizeMod32));
        state.v1_2 |= (ulong)((half1 << sizeMod32) | (half1 >> (32 - sizeMod32))) << 32;

        half0 = (uint)(state.v1_3 & 0xffffffff);
        half1 = (uint)(state.v1_3 >> 32);
        state.v1_3 = (half0 << sizeMod32) | (half0 >> (32 - sizeMod32));
        state.v1_3 |= (ulong)((half1 << sizeMod32) | (half1 >> (32 - sizeMod32))) << 32;

        bytes.Slice(0, remainderOffset).CopyTo(packet);

        if ((sizeMod32 & 16) != 0)
        {
            for (int i = 0; i < 4; i++)
                packet[28 + i] = bytes[(remainderOffset + i + sizeMod4) - 4];
        }
        else if (sizeMod4 != 0)
        {
            packet[16] = bytes[remainderOffset];
            packet[17] = bytes[remainderOffset + (sizeMod4 >> 1)];
            packet[18] = bytes[(remainderOffset + sizeMod4) - 1];
        }

        UpdatePacket(packet, 0, ref state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Finalize64(HighwayHashState state)
    {
        for (int i = 0; i < 4; i++)
            PermuteAndUpdate(ref state);

        return state.v0_0 + state.v1_0 + state.mul0_0 + state.mul1_0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PermuteAndUpdate(ref HighwayHashState state)
    {
        Update(
            (state.v0_2 >> 32) | (state.v0_2 << 32),
            (state.v0_3 >> 32) | (state.v0_3 << 32),
            (state.v0_0 >> 32) | (state.v0_0 << 32),
            (state.v0_1 >> 32) | (state.v0_1 << 32),
            ref state);
    }
}