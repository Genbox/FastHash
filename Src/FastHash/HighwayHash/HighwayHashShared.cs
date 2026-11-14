// C# port of google/highwayhash (f8381f3). Copyright 2017 Google Inc. All Rights Reserved.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;

namespace Genbox.FastHash.HighwayHash;

internal static class HighwayHashShared
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Reset(ulong key0, ulong key1, ulong key2, ulong key3, ref HighwayHashState state)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Update(ulong lane0, ulong lane1, ulong lane2, ulong lane3, ref HighwayHashState state)
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
    internal static void UpdateIndex(ulong input, ref HighwayHashState state)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Finalize64(HighwayHashState state)
    {
        for (int i = 0; i < 4; i++)
            PermuteAndUpdate(ref state);

        return state.v0_0 + state.v1_0 + state.mul0_0 + state.mul1_0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void PermuteAndUpdate(ref HighwayHashState state) => Update((state.v0_2 >> 32) | (state.v0_2 << 32),
        (state.v0_3 >> 32) | (state.v0_3 << 32),
        (state.v0_0 >> 32) | (state.v0_0 << 32),
        (state.v0_1 >> 32) | (state.v0_1 << 32),
        ref state);
}