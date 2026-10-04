// C# port of google/cityhash 1.1.1 (4726e30). Copyright (c) 2011 Google, Inc.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using static Genbox.FastHash.CityHash.CityHashShared;
using static Genbox.FastHash.CityHash.CityHashConstants;

namespace Genbox.FastHash.CityHash;

/// <summary>Provides 128-bit CityHash functions.</summary>
public static class CityHash128
{
    /// <summary>Computes the hash of a 64-bit value.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 128-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input)
    {
        const uint len = 8;
        ulong a = K0;
        ulong b = K1;

        a = ShiftMix(a * K1) * K1;

        ulong mul = K2 + (len * 2);
        ulong a0 = input + K2;
        ulong b0 = input;
        ulong c0 = (RotateRight(b0, 37) * mul) + a0;
        ulong d0 = (RotateRight(a0, 25) + b0) * mul;
        ulong c = (b * K1) + HashLen16(c0, d0, mul);

        ulong d = ShiftMix(a + input);
        a = HashLen16(a, c);
        b = HashLen16(d, b);
        return new UInt128(HashLen16(b, a), a ^ b);
    }

    /// <summary>Computes the hash of data with the default seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 128-bit hash.</returns>
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data)
    {
        uint len = (uint)data.Length;

        if (len >= 16)
        {
            UInt128 seed = new UInt128(Read64(data, 8) + K0, Read64(data));
            return CityHash128WithSeed(data.Slice(16, (int)(len - 16)), len - 16, seed);
        }

        return CityHash128WithSeed(data, len, new UInt128(K1, K0));
    }

    /// <summary>Computes the hash of data with a seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 128-bit hash.</returns>
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, UInt128 seed) => CityHash128WithSeed(data, (uint)data.Length, seed);

    // A subroutine for CityHash128().  Returns a decent 128-bit hash for strings
    // of any length representable in signed long.  Based on City and Murmur.
    private static UInt128 CityMurmur(ReadOnlySpan<byte> s, uint len, UInt128 seed)
    {
        ulong a = seed.Low;
        ulong b = seed.High;
        ulong c;
        ulong d;

        if (len <= 16)
        {
            a = ShiftMix(a * K1) * K1;
            c = (b * K1) + HashLen0to16(s, len);
            d = ShiftMix(a + (len >= 8 ? Read64(s) : c));
        }
        else
        {
            c = HashLen16(Read64(s, len - 8) + K1, a);
            d = HashLen16(b + len, c + Read64(s, len - 16));
            a += d;

            // len > 16 here, so do...while is safe
            uint offset = 0;

            do
            {
                a ^= ShiftMix(Read64(s, offset) * K1) * K1;
                a *= K1;
                b ^= a;
                c ^= ShiftMix(Read64(s, offset + 8) * K1) * K1;
                c *= K1;
                d ^= c;
                offset += 16;
                len -= 16;
            } while (len > 16);
        }

        a = HashLen16(a, c);
        b = HashLen16(d, b);
        return new UInt128(HashLen16(b, a), a ^ b);
    }

    private static UInt128 CityHash128WithSeed(ReadOnlySpan<byte> s, uint len, UInt128 seed)
    {
        if (len < 128)
            return CityMurmur(s, len, seed);

        // We expect len >= 128 to be the common case.  Keep 56 bytes of state:
        // v, w, x, y, and z.
        ulong x = seed.Low;
        ulong y = seed.High;
        ulong z = len * K1;
        ulong vLow = (RotateRight(y ^ K1, 49) * K1) + Read64(s);
        UInt128 v = new UInt128((RotateRight(vLow, 42) * K1) + Read64(s, 8), vLow);
        UInt128 w = new UInt128(RotateRight(x + Read64(s, 88), 53) * K1, (RotateRight(y + z, 35) * K1) + x);

        // This is the same inner loop as CityHash64(), manually unrolled.
        uint offset = 0;

        do
        {
            x = RotateRight(x + y + v.Low + Read64(s, offset + 8), 37) * K1;
            y = RotateRight(y + v.High + Read64(s, offset + 48), 42) * K1;
            x ^= w.High;
            y += v.Low + Read64(s, offset + 40);
            z = RotateRight(z + w.Low, 33) * K1;
            v = WeakHashLen32WithSeeds(s, offset + 0, v.High * K1, x + w.Low);
            w = WeakHashLen32WithSeeds(s, offset + 32, z + w.High, y + Read64(s, offset + 16));
            Swap(ref z, ref x);
            offset += 64;
            x = RotateRight(x + y + v.Low + Read64(s, offset + 8), 37) * K1;
            y = RotateRight(y + v.High + Read64(s, offset + 48), 42) * K1;
            x ^= w.High;
            y += v.Low + Read64(s, offset + 40);
            z = RotateRight(z + w.Low, 33) * K1;
            v = WeakHashLen32WithSeeds(s, offset, v.High * K1, x + w.Low);
            w = WeakHashLen32WithSeeds(s, offset + 32, z + w.High, y + Read64(s, offset + 16));
            Swap(ref z, ref x);
            offset += 64;
            len -= 128;
        } while (len >= 128);

        x += RotateRight(v.Low + z, 49) * K0;
        y = (y * K0) + RotateRight(w.High, 37);
        z = (z * K0) + RotateRight(w.Low, 27);
        w = new UInt128(w.High, w.Low * 9);
        v = new UInt128(v.High, v.Low * K0);

        // If 0 < len < 128, hash up to 4 chunks of 32 bytes each from the end of s.
        for (uint tail_done = 0; tail_done < len;)
        {
            tail_done += 32;
            y = (RotateRight(x + y, 42) * K0) + v.High;
            w = new UInt128(w.High, w.Low + Read64(s, ((offset + len) - tail_done) + 16));
            x = (x * K0) + w.Low;
            z += w.High + Read64(s, (offset + len) - tail_done);
            w = new UInt128(w.High + v.Low, w.Low);
            v = WeakHashLen32WithSeeds(s, (offset + len) - tail_done, v.Low + z, v.High);
            v = new UInt128(v.High, v.Low * K0);
        }

        // At this point our 56 bytes of state should contain more than
        // enough information for a strong 128-bit hash.  We use two
        // different 56-byte-to-8-byte hashes to get a 16-byte final result.
        x = HashLen16(x, v.Low);
        y = HashLen16(y + z, w.Low);
        return new UInt128(HashLen16(x + w.High, y + v.High), HashLen16(x + v.High, w.High) + y);
    }
}