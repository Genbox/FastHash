using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Genbox.FastHash.PolymurHash.PolymurConstants;

namespace Genbox.FastHash.PolymurHash;

/// <summary>Provides 64-bit Polymur hash computations.</summary>
public static class Polymur2Hash64
{
    private static readonly Parameters _params = CreateParams(0);

    /// <summary>Computes a hash index for <paramref name="input"/> using the default seed.</summary>
    /// <param name="input">The value to hash.</param>
    /// <returns>The 64-bit hash index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input) => ComputeIndex(input, 0);

    /// <summary>Computes a hash index for <paramref name="input"/> using <paramref name="seed"/>.</summary>
    /// <param name="input">The value to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash index.</returns>
    public static ulong ComputeIndex(ulong input, ulong seed)
    {
        Parameters p = seed == 0 ? _params : CreateParams(seed);
        return ComputeIndex(input, in p);
    }

    /// <summary>Computes a hash index for <paramref name="input"/> using precomputed <paramref name="parameters"/>.</summary>
    /// <param name="input">The value to hash.</param>
    /// <param name="parameters">The seed-derived hash parameters.</param>
    /// <returns>The 64-bit hash index.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, PolymurHashParams parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        return ComputeIndex(input, in parameters.Parameters);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong ComputeIndex(ulong input, in Parameters p)
    {
        const int len = 8;
        ulong m0 = input & 0x00ffffffffffffffUL;
        ulong m1 = m0;
        ulong m2 = input >> 8;
        UInt128 t0 = polymur_mul128(p.k2 + m0, p.k7 + m1);
        UInt128 t1 = polymur_mul128(p.k + m2, p.k3 + len);
        ulong h = polymur_red611(polymur_add128(t0, t1));
        return polymur_mix(h) + p.s;
    }

    /// <summary>Computes a 64-bit hash for <paramref name="data"/> using the default seed.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes a 64-bit hash for <paramref name="data"/> using <paramref name="seed"/>.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed) => ComputeHash(data, seed, 0);

    /// <summary>Computes a 64-bit hash for <paramref name="data"/> using a seed and tweak.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <param name="tweak">An additional value incorporated into the hash.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed, ulong tweak)
    {
        Parameters p = seed == 0 ? _params : CreateParams(seed);
        return polymur_hash(data, data.Length, in p, tweak);
    }

    /// <summary>Computes a 64-bit hash for <paramref name="data"/> using precomputed <paramref name="parameters"/>.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="parameters">The seed-derived hash parameters.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeHash(ReadOnlySpan<byte> data, PolymurHashParams parameters) => ComputeHash(data, parameters, 0);

    /// <summary>Computes a 64-bit hash for <paramref name="data"/> using precomputed parameters and a tweak.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="parameters">The seed-derived hash parameters.</param>
    /// <param name="tweak">An additional value incorporated into the hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, PolymurHashParams parameters, ulong tweak)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        return polymur_hash(data, data.Length, in parameters.Parameters, tweak);
    }

    internal static Parameters CreateParams(ulong seed) => polymur_init_params_from_seed(seed);

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
            v |= (ulong)buf[offset + (len / 2)] << (8 * (len / 2));
            v |= (ulong)buf[(offset + len) - 1] << (8 * (len - 1));
            return v;
        }

        ulong lo = polymur_load_le_u32(buf, offset + 0);
        ulong hi = polymur_load_le_u32(buf, (offset + len) - 4);
        return lo | (hi << (8 * (len - 4)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 polymur_add128(UInt128 a, UInt128 b)
    {
        ulong low = a.Low + b.Low;
        return new UInt128(low, a.High + b.High + (low < b.Low ? 1UL : 0UL));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 polymur_mul128(ulong a, ulong b) => Multiply64To128(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong polymur_red611(UInt128 x) => (x.Low & POLYMUR_P611) + ((x.Low >> 61) | (x.High << 3));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong polymur_extrared611(ulong x) => (x & POLYMUR_P611) + (x >> 61);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong polymur_mix(ulong x) => JM_xmxmx_Mx2_64(x);

    private static Parameters polymur_init_params(ulong k_seed, ulong s_seed)
    {
        ulong s = s_seed ^ POLYMUR_ARBITRARY1; // People love to pass zero.

        // POLYMUR_POW37[i] = 37^(2^i) mod (2^61 - 1)
        // Could be replaced by a 512 byte LUT, costs ~400 byte overhead but 2x
        // faster seeding. However, seeding is rather rare, so I chose not to.
        Span<ulong> POLYMUR_POW37 = stackalloc ulong[64];
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
            ulong key = polymur_extrared611(k);
            ulong k2 = polymur_extrared611(polymur_red611(polymur_mul128(key, key)));
            ulong k3 = polymur_red611(polymur_mul128(key, k2));
            ulong k4 = polymur_red611(polymur_mul128(k2, k2));
            ulong k7 = polymur_extrared611(polymur_red611(polymur_mul128(k3, k4)));

            if (k7 < (1UL << 60) - (1UL << 56))
                return new Parameters(key, k2, k3, k7, s);

            // Our key space is log2(totient(2^61 - 2) * (2^60-2^56)/2^61) ~= 57.4 bits.
        }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Parameters polymur_init_params_from_seed(ulong seed) => polymur_init_params(polymur_mix(seed + POLYMUR_ARBITRARY3), polymur_mix(seed + POLYMUR_ARBITRARY4));

    private static ulong polymur_hash_poly611(ReadOnlySpan<byte> buf, int len, in Parameters p, ulong tweak)
    {
        ulong poly_acc = tweak;
        int bufPtr = 0;

        if (len <= 7)
        {
            ulong m0 = polymur_load_le_u64_0_8(buf, len);
            return poly_acc + polymur_red611(polymur_mul128(p.k + m0, p.k2 + (uint)len));
        }

        ulong k3 = p.k3;
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
                ulong m0 = polymur_load_le_u64(buf, bufPtr) & 0x00ffffffffffffffUL;
                ulong m1 = polymur_load_le_u64(buf, bufPtr + 7) & 0x00ffffffffffffffUL;
                ulong m2 = polymur_load_le_u64(buf, bufPtr + 14) & 0x00ffffffffffffffUL;
                ulong m3 = polymur_load_le_u64(buf, bufPtr + 21) & 0x00ffffffffffffffUL;
                ulong m4 = polymur_load_le_u64(buf, bufPtr + 28) & 0x00ffffffffffffffUL;
                ulong m5 = polymur_load_le_u64(buf, bufPtr + 35) & 0x00ffffffffffffffUL;
                ulong m6 = polymur_load_le_u64(buf, bufPtr + 42) & 0x00ffffffffffffffUL;

                UInt128 t0 = polymur_mul128(p.k + m0, k6 + m1);
                UInt128 t1 = polymur_mul128(p.k2 + m2, k5 + m3);
                UInt128 t2 = polymur_mul128(k3 + m4, k4 + m5);
                UInt128 t3 = polymur_mul128(h + m6, p.k7);
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
            ulong m0 = polymur_load_le_u64(buf, bufPtr) & 0x00ffffffffffffffUL;
            ulong m1 = polymur_load_le_u64(buf, bufPtr + ((len - 7) / 2)) & 0x00ffffffffffffffUL;
            ulong m2 = polymur_load_le_u64(buf, (bufPtr + len) - 8) >> 8;
            UInt128 t0 = polymur_mul128(p.k2 + m0, p.k7 + m1);
            UInt128 t1 = polymur_mul128(p.k + m2, k3 + (uint)len);

            if (len <= 21)
                return poly_acc + polymur_red611(polymur_add128(t0, t1));

            ulong m3 = polymur_load_le_u64(buf, bufPtr + 7) & 0x00ffffffffffffffUL;
            ulong m4 = polymur_load_le_u64(buf, bufPtr + 14) & 0x00ffffffffffffffUL;
            ulong m5 = polymur_load_le_u64(buf, (bufPtr + len) - 21) & 0x00ffffffffffffffUL;
            ulong m6 = polymur_load_le_u64(buf, (bufPtr + len) - 14) & 0x00ffffffffffffffUL;
            ulong t0r = polymur_red611(t0);
            UInt128 t2 = polymur_mul128(p.k2 + m3, p.k7 + m4);
            UInt128 t3 = polymur_mul128(t0r + m5, k4 + m6);
            UInt128 s = polymur_add128(polymur_add128(t1, t2), t3);
            return poly_acc + polymur_red611(s);
        }

        ulong m = polymur_load_le_u64_0_8(buf, len, bufPtr);
        return poly_acc + polymur_red611(polymur_mul128(p.k + m, p.k2 + (uint)len));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong polymur_hash(ReadOnlySpan<byte> buf, int len, in Parameters p, ulong tweak)
    {
        ulong h = polymur_hash_poly611(buf, len, in p, tweak);
        return polymur_mix(h) + p.s;
    }

    [StructLayout(LayoutKind.Auto)]
    internal readonly struct Parameters
    {
        internal Parameters(ulong k, ulong k2, ulong k3, ulong k7, ulong s)
        {
            this.k = k;
            this.k2 = k2;
            this.k3 = k3;
            this.k7 = k7;
            this.s = s;
        }

        internal readonly ulong k;
        internal readonly ulong k2;
        internal readonly ulong k3;
        internal readonly ulong k7;
        internal readonly ulong s;
    }
}