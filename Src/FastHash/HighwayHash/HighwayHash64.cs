// C# port of google/highwayhash (f8381f3). Copyright 2017 Google Inc. All Rights Reserved.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using static Genbox.FastHash.HighwayHash.HighwayHashShared;

namespace Genbox.FastHash.HighwayHash;

/// <summary>Provides the 64-bit HighwayHash algorithm.</summary>
public static class HighwayHash64
{
    /// <summary>Computes the hash of a 64-bit value using the default key.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input) => ComputeIndex(input, HighwayHashConstants.DefaultKey0, HighwayHashConstants.DefaultKey1, HighwayHashConstants.DefaultKey2, HighwayHashConstants.DefaultKey3);

    /// <summary>Computes the hash of a 64-bit value.</summary>
    /// <param name="input">The value to hash.</param>
    /// <param name="seed1">The first key word.</param>
    /// <param name="seed2">The second key word.</param>
    /// <param name="seed3">The third key word.</param>
    /// <param name="seed4">The fourth key word.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed1, ulong seed2, ulong seed3, ulong seed4)
    {
#if NET8_0_OR_GREATER
        if (HighwayHashAvx2.IsSupported)
            return HighwayHashAvx2.HashIndex(input, seed1, seed2, seed3, seed4);
#endif

        HighwayHashState state = new HighwayHashState();
        Reset(seed1, seed2, seed3, seed4, ref state);
        UpdateIndex(input, ref state);
        return Finalize64(state);
    }

    /// <summary>Computes the hash of a 64-bit value.</summary>
    /// <param name="input">The value to hash.</param>
    /// <param name="keys">The key words; the first four are used.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="keys" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="keys" /> has fewer than four elements.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong[] keys)
    {
        ValidateKeys(keys);
        return ComputeIndex(input, keys[0], keys[1], keys[2], keys[3]);
    }

    /// <summary>Computes the hash of a byte sequence using the default key.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, HighwayHashConstants.DefaultKey0, HighwayHashConstants.DefaultKey1, HighwayHashConstants.DefaultKey2, HighwayHashConstants.DefaultKey3);

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed1">The first key word.</param>
    /// <param name="seed2">The second key word.</param>
    /// <param name="seed3">The third key word.</param>
    /// <param name="seed4">The fourth key word.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed1, ulong seed2, ulong seed3, ulong seed4)
    {
#if NET8_0_OR_GREATER
        if (HighwayHashAvx2.IsSupported)
            return HighwayHashAvx2.Hash(data, seed1, seed2, seed3, seed4);
#endif

        HighwayHashState state = new HighwayHashState();
        ProcessAll(data, seed1, seed2, seed3, seed4, ref state);
        return Finalize64(state);
    }

    /// <summary>Computes the hash of a byte sequence.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="keys">The key words; the first four are used.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="keys" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="keys" /> has fewer than four elements.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong[] keys)
    {
        ValidateKeys(keys);
        return ComputeHash(data, keys[0], keys[1], keys[2], keys[3]);
    }

    internal static void ValidateKeys(ulong[]? keys)
    {
        if (keys == null)
            throw new ArgumentNullException(nameof(keys));

        if (keys.Length < 4)
            throw new ArgumentException("HighwayHash keys must contain at least four 64-bit words.", nameof(keys));
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
    private static void UpdatePacket(ReadOnlySpan<byte> packet, int offset, ref HighwayHashState state) => Update(Read64(packet, offset), Read64(packet, offset + 8), Read64(packet, offset + 16), Read64(packet, offset + 24), ref state);

    private static void UpdateRemainder(ReadOnlySpan<byte> bytes, int sizeMod32, ref HighwayHashState state)
    {
        int sizeMod4 = sizeMod32 & 3;
        int remainderOffset = sizeMod32 & ~3;
        Span<byte> packet = stackalloc byte[32];
        packet.Clear();

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

}