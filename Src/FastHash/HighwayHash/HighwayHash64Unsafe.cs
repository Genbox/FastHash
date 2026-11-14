// C# port of google/highwayhash (f8381f3). Copyright 2017 Google Inc. All Rights Reserved.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using static Genbox.FastHash.HighwayHash.HighwayHashShared;

namespace Genbox.FastHash.HighwayHash;

/// <summary>Provides pointer-based access to the 64-bit HighwayHash algorithm.</summary>
public static class HighwayHash64Unsafe
{
    /// <summary>Computes the hash of an unmanaged byte sequence using the default key.</summary>
    /// <param name="data">A pointer to at least <paramref name="size" /> bytes, or any pointer when <paramref name="size" /> is zero.</param>
    /// <param name="size">The non-negative number of bytes to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size" /> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int size) => ComputeHash(data, size, HighwayHashConstants.DefaultKey0, HighwayHashConstants.DefaultKey1, HighwayHashConstants.DefaultKey2, HighwayHashConstants.DefaultKey3);

    /// <summary>Computes the hash of an unmanaged byte sequence.</summary>
    /// <param name="data">A pointer to at least <paramref name="size" /> bytes, or any pointer when <paramref name="size" /> is zero.</param>
    /// <param name="size">The non-negative number of bytes to hash.</param>
    /// <param name="seed1">The first key word.</param>
    /// <param name="seed2">The second key word.</param>
    /// <param name="seed3">The third key word.</param>
    /// <param name="seed4">The fourth key word.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size" /> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int size, ulong seed1, ulong seed2, ulong seed3, ulong seed4)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));

        return ComputeHashCore(data, size, seed1, seed2, seed3, seed4);
    }

    /// <summary>Computes the hash of an unmanaged byte sequence.</summary>
    /// <param name="data">A pointer to at least <paramref name="size" /> bytes.</param>
    /// <param name="size">The non-negative number of bytes to hash.</param>
    /// <param name="keys">The key words; the first four are used.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="size" /> is negative.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="keys" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="keys" /> has fewer than four elements.</exception>
    public static unsafe ulong ComputeHash(byte* data, int size, ulong[] keys)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));

        HighwayHash64.ValidateKeys(keys);
        return ComputeHashCore(data, size, keys[0], keys[1], keys[2], keys[3]);
    }

    private static unsafe ulong ComputeHashCore(byte* data, int size, ulong seed1, ulong seed2, ulong seed3, ulong seed4)
    {
#if NET8_0_OR_GREATER
        if (HighwayHashAvx2.IsSupported)
            return HighwayHashAvx2.Hash(ref *data, size, seed1, seed2, seed3, seed4);
#endif

        HighwayHashState state = new HighwayHashState();
        ProcessAll(data, (uint)size, seed1, seed2, seed3, seed4, ref state);
        return Finalize64(state);
    }

    private static unsafe void ProcessAll(byte* data, uint size, ulong key0, ulong key1, ulong key2, ulong key3, ref HighwayHashState state)
    {
        uint i;
        Reset(key0, key1, key2, key3, ref state);

        for (i = 0; i + 32 <= size; i += 32)
            HighwayHashUpdatePacket(data + i, ref state);

        if ((size & 31) != 0)
            HighwayHashUpdateRemainder(data + i, size & 31, ref state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void HighwayHashUpdatePacket(byte* packet, ref HighwayHashState state) => Update(Read64(packet + 0), Read64(packet + 8), Read64(packet + 16), Read64(packet + 24), ref state);

    private static unsafe void HighwayHashUpdateRemainder(byte* bytes, uint size_mod32, ref HighwayHashState state)
    {
        int i;
        uint size_mod4 = size_mod32 & 3;
        byte* remainder = bytes + (size_mod32 & ~3);
        byte* packet = stackalloc byte[32];
        Unsafe.InitBlockUnaligned(packet, 0, 32);

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
                packet[28 + i] = remainder[(i + size_mod4) - 4];
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

}