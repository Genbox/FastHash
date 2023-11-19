/*
    PolymurHash version 2.0

    Copyright (c) 2023 Orson Peters

    This software is provided 'as-is', without any express or implied warranty. In
    no event will the authors be held liable for any damages arising from the use of
    this software.

    Permission is granted to anyone to use this software for any purpose, including
    commercial applications, and to alter it and redistribute it freely, subject to
    the following restrictions:

    1. The origin of this software must not be misrepresented; you must not claim
        that you wrote the original software. If you use this software in a product,
        an acknowledgment in the product documentation would be appreciated but is
        not required.

    2. Altered source versions must be plainly marked as such, and must not be
        misrepresented as being the original software.

    3. This notice may not be removed or altered from any source distribution.
*/

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Genbox.FastHash.PolymurHash.PolymurConstants;

namespace Genbox.FastHash.PolymurHash;

public static class Polymur2Hash64
{
    private static PolymurHashParams _params;

    static Polymur2Hash64()
    {
        //Note: Due to the static API of FastHash, for perf reasons we have to pre-generate the polynomials based on a static seed here.
        //We then use the "tweak" parameter below with the seed instead. This might not give the same security guarantees, but it is our only choice right now.
        _params = new PolymurHashParams();
        polymur_init_params_from_seed(ref _params, 0xfedbca9876543210UL);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        ulong lo = unchecked((uint)input);
        ulong hi = input >> 32;
        return lo | (hi << 32);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed = 0)
    {
        return polymur_hash(data, data.Length, ref _params, seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint polymur_load_le_u32(ReadOnlySpan<byte> p, int offset) => Read32(p, offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong polymur_load_le_u64(ReadOnlySpan<byte> p, int offset) => Read64(p, offset);

    // Loads 0 to 8 bytes from buf with length len as a 64-bit little-endian integer.
    private static ulong polymur_load_le_u64_0_8(ReadOnlySpan<byte> buf, int len, int offset = 0)
    {
        if (len < 4)
        {
            if (len == 0)
                return 0;

            ulong v = buf[offset + 0];
            v |= (ulong)buf[offset + len / 2] << 8 * (len / 2);
            v |= (ulong)buf[offset + len - 1] << 8 * (len - 1);
            return v;
        }

        ulong lo = polymur_load_le_u32(buf, offset + 0);
        ulong hi = polymur_load_le_u32(buf, offset + len - 4);
        return lo | (hi << 8 * (len - 4));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 polymur_add128(UInt128 a, UInt128 b)
    {
        a.Low += b.Low;
        a.High += b.High + (a.Low < b.Low ? 1UL : 0UL);
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 polymur_mul128(ulong a, ulong b)
    {
        UInt128 ret;
        ret.High = BigMul(a, b, out ret.Low);
        return ret;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong polymur_red611(UInt128 x)
    {
        // return ((( ulong) x.lo) & POLYMUR_P611) + __shiftright128(x.lo, x.hi, 61);
        return (x.Low & POLYMUR_P611) + ((x.Low >> 61) | (x.High << 3));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong polymur_extrared611(ulong x) => (x & POLYMUR_P611) + (x >> 61);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong polymur_mix(ulong x) => Mx2_64(x);

    private static void polymur_init_params(ref PolymurHashParams p, ulong k_seed, ulong s_seed)
    {
        p.s = s_seed ^ POLYMUR_ARBITRARY1; // People love to pass zero.

        // POLYMUR_POW37[i] = 37^(2^i) mod (2^61 - 1)
        // Could be replaced by a 512 byte LUT, costs ~400 byte overhead but 2x
        // faster seeding. However, seeding is rather rare, so I chose not to.
        ulong[] POLYMUR_POW37 = new ulong[64];
        POLYMUR_POW37[0] = 37;
        POLYMUR_POW37[32] = 559096694736811184UL;
        for (int i = 0; i < 31; ++i)
        {
            POLYMUR_POW37[i + 1] = polymur_extrared611(polymur_red611(polymur_mul128(POLYMUR_POW37[i], POLYMUR_POW37[i])));
            POLYMUR_POW37[i + 33] = polymur_extrared611(polymur_red611(polymur_mul128(POLYMUR_POW37[i + 32], POLYMUR_POW37[i + 32])));
        }

        while (true)
        {
            // Choose a random exponent coprime to 2^61 - 2. ~35.3% success rate.
            k_seed += POLYMUR_ARBITRARY2;
            ulong e = (k_seed >> 3) | 1; // e < 2^61, odd.
            if (e % 3 == 0) continue;
            if (e % 5 == 0 || e % 7 == 0) continue;
            if (e % 11 == 0 || e % 13 == 0 || e % 31 == 0) continue;
            if (e % 41 == 0 || e % 61 == 0 || e % 151 == 0 || e % 331 == 0 || e % 1321 == 0) continue;

            // Compute k = 37^e mod 2^61 - 1. Since e is coprime with the order of
            // the multiplicative group mod 2^61 - 1 and 37 is a generator, this
            // results in another generator of the group.
            ulong ka = 1, kb = 1;
            for (int i = 0; e != 0; i += 2, e >>= 2)
            {
                if ((e & 1) != 0) ka = polymur_extrared611(polymur_red611(polymur_mul128(ka, POLYMUR_POW37[i])));
                if ((e & 2) != 0) kb = polymur_extrared611(polymur_red611(polymur_mul128(kb, POLYMUR_POW37[i + 1])));
            }
            ulong k = polymur_extrared611(polymur_red611(polymur_mul128(ka, kb)));

            // ~46.875% success rate. Bound on k^7 needed for efficient reduction.
            p.k = polymur_extrared611(k);
            p.k2 = polymur_extrared611(polymur_red611(polymur_mul128(p.k, p.k)));
            ulong k3 = polymur_red611(polymur_mul128(p.k, p.k2));
            ulong k4 = polymur_red611(polymur_mul128(p.k2, p.k2));
            p.k7 = polymur_extrared611(polymur_red611(polymur_mul128(k3, k4)));

            if (p.k7 < (1UL << 60) - (1UL << 56))
                break;

            // Our key space is log2(totient(2^61 - 2) * (2^60-2^56)/2^61) ~= 57.4 bits.
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void polymur_init_params_from_seed(ref PolymurHashParams p, ulong seed)
    {
        polymur_init_params(ref p, polymur_mix(seed + POLYMUR_ARBITRARY3), polymur_mix(seed + POLYMUR_ARBITRARY4));
    }

    private static ulong polymur_hash_poly611(ReadOnlySpan<byte> buf, int len, ref PolymurHashParams p, ulong tweak)
    {
        Span<ulong> m = stackalloc ulong[7];
        ulong poly_acc = tweak;
        int bufPtr = 0;

        if (len <= 7)
        {
            m[0] = polymur_load_le_u64_0_8(buf, len);
            return poly_acc + polymur_red611(polymur_mul128(p.k + m[0], p.k2 + (uint)len));
        }

        ulong k3 = polymur_red611(polymur_mul128(p.k, p.k2));
        ulong k4 = polymur_red611(polymur_mul128(p.k2, p.k2));
        if (len >= 50)
        {
            ulong k5 = polymur_extrared611(polymur_red611(polymur_mul128(p.k, k4)));
            ulong k6 = polymur_extrared611(polymur_red611(polymur_mul128(p.k2, k4)));
            k3 = polymur_extrared611(k3);
            k4 = polymur_extrared611(k4);
            ulong h = 0;
            do
            {
                for (int i = 0; i < 7; ++i)
                    m[i] = polymur_load_le_u64(buf, bufPtr + 7 * i) & 0x00ffffffffffffffUL;

                UInt128 t0 = polymur_mul128(p.k + m[0], k6 + m[1]);
                UInt128 t1 = polymur_mul128(p.k2 + m[2], k5 + m[3]);
                UInt128 t2 = polymur_mul128(k3 + m[4], k4 + m[5]);
                UInt128 t3 = polymur_mul128(h + m[6], p.k7);
                UInt128 s = polymur_add128(polymur_add128(t0, t1), polymur_add128(t2, t3));
                h = polymur_red611(s);
                len -= 49;
                bufPtr += 49;
            } while (len >= 50);

            ulong k14 = polymur_red611(polymur_mul128(p.k7, p.k7));
            ulong hk14 = polymur_red611(polymur_mul128(polymur_extrared611(h), k14));
            poly_acc += polymur_extrared611(hk14);
        }

        if (len >= 8)
        {
            m[0] = polymur_load_le_u64(buf, bufPtr) & 0x00ffffffffffffffUL;
            m[1] = polymur_load_le_u64(buf, bufPtr + (len - 7) / 2) & 0x00ffffffffffffffUL;
            m[2] = polymur_load_le_u64(buf, bufPtr + len - 8) >> 8;
            UInt128 t0 = polymur_mul128(p.k2 + m[0], p.k7 + m[1]);
            UInt128 t1 = polymur_mul128(p.k + m[2], k3 + (uint)len);

            if (len <= 21)
                return poly_acc + polymur_red611(polymur_add128(t0, t1));

            m[3] = polymur_load_le_u64(buf, bufPtr + 7) & 0x00ffffffffffffffUL;
            m[4] = polymur_load_le_u64(buf, bufPtr + 14) & 0x00ffffffffffffffUL;
            m[5] = polymur_load_le_u64(buf, bufPtr + len - 21) & 0x00ffffffffffffffUL;
            m[6] = polymur_load_le_u64(buf, bufPtr + len - 14) & 0x00ffffffffffffffUL;
            ulong t0r = polymur_red611(t0);
            UInt128 t2 = polymur_mul128(p.k2 + m[3], p.k7 + m[4]);
            UInt128 t3 = polymur_mul128(t0r + m[5], k4 + m[6]);
            UInt128 s = polymur_add128(polymur_add128(t1, t2), t3);
            return poly_acc + polymur_red611(s);
        }

        m[0] = polymur_load_le_u64_0_8(buf, len, bufPtr);
        return poly_acc + polymur_red611(polymur_mul128(p.k + m[0], p.k2 + (uint)len));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong polymur_hash(ReadOnlySpan<byte> buf, int len, ref PolymurHashParams p, ulong tweak)
    {
        ulong h = polymur_hash_poly611(buf, len, ref p, tweak);
        return polymur_mix(h) + p.s;
    }

    [StructLayout(LayoutKind.Auto)]
    private struct PolymurHashParams
    {
        public ulong k;
        public ulong k2;
        public ulong k7;
        public ulong s;
    }
}