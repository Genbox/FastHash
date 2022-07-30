//Ported to C# by Ian Qvist
//Source: https://github.com/google/highwayhash/

namespace Genbox.FastHash.HighwayHash;

public static class HighwayHash64
{
    private const int kNumLanes = 4;
    private static readonly ulong[] v0 = new ulong[4];
    private static readonly ulong[] v1 = new ulong[4];
    private static readonly ulong[] mul0 = new ulong[4];
    private static readonly ulong[] mul1 = new ulong[4];

    public static ulong ComputeHash(byte[] data, ulong seed0 = 0, ulong seed1 = 0, ulong seed2 = 0, ulong seed3 = 0)
    {
        ulong[] init0 =
        {
            0xdbe6d5d5fe4cce2ful, 0xa4093822299f31d0ul,
            0x13198a2e03707344ul, 0x243f6a8885a308d3ul
        };
        ulong[] init1 =
        {
            0x3bd39e10cb0ef593ul, 0xc0acf169b5f18a8cul,
            0xbe5466cf34e90c6cul, 0x452821e638d01377ul
        };

        ulong[] keys = { seed0, seed1, seed2, seed3 };

        ulong[] permuted_keys = new ulong[4];
        Permute(keys, permuted_keys);
        Copy(init0, mul0);
        Copy(init1, mul1);
        Xor(init0, keys, v0);
        Xor(init1, permuted_keys, v1);

        ulong[] packets = new ulong[data.Length / 8 + 1];

        unsafe
        {
            fixed (byte* ptr = data)
            {
                ulong* uptr = (ulong*)ptr;

                for (int i = 0; i < packets.Length; i++)
                    packets[i] = uptr[i];
            }
        }

        Update(packets);

        PermuteAndUpdate();
        PermuteAndUpdate();
        PermuteAndUpdate();
        PermuteAndUpdate();

        return v0[0] + v1[0] + mul0[0] + mul1[0];
    }

    private static void Update(ulong[] packets)
    {
        Add(packets, v1);
        Add(mul0, v1);

        // (Loop is faster than unrolling)
        for (int lane = 0; lane < kNumLanes; ++lane)
        {
            uint v1_32 = (uint)v1[lane];
            mul0[lane] ^= v1_32 * (v0[lane] >> 32);
            v0[lane] += mul1[lane];
            uint v0_32 = (uint)v0[lane];
            mul1[lane] ^= v0_32 * (v1[lane] >> 32);
        }

        ZipperMergeAndAdd(v1[0], v1[1], ref v0[0], ref v0[1]);
        ZipperMergeAndAdd(v1[2], v1[3], ref v0[2], ref v0[3]);

        ZipperMergeAndAdd(v0[0], v0[1], ref v1[0], ref v1[1]);
        ZipperMergeAndAdd(v0[2], v0[3], ref v1[2], ref v1[3]);
    }

    private static ulong MASK(ulong v, int bytes) => v & (0xFFul << (bytes * 8));

    private static void Copy(ulong[] source, ulong[] dest)
    {
        for (int lane = 0; lane < kNumLanes; ++lane)
            dest[lane] = source[lane];
    }

    private static void Add(ulong[] source, ulong[] dest)
    {
        for (int lane = 0; lane < kNumLanes; ++lane)
            dest[lane] += source[lane];
    }

    private static void Xor(ulong[] op1, ulong[] op2, ulong[] dest)
    {
        for (int lane = 0; lane < kNumLanes; ++lane)
            dest[lane] = op1[lane] ^ op2[lane];
    }

    private static void ZipperMergeAndAdd(ulong v0, ulong v1, ref ulong add0, ref ulong add1)
    {
        add0 += ((MASK(v0, 3) + MASK(v1, 4)) >> 24) +
                ((MASK(v0, 5) + MASK(v1, 6)) >> 16) + MASK(v0, 2) +
                (MASK(v0, 1) << 32) + (MASK(v1, 7) >> 8) + (v0 << 56);

        add1 += ((MASK(v1, 3) + MASK(v0, 4)) >> 24) + MASK(v1, 2) +
                (MASK(v1, 5) >> 16) + (MASK(v1, 1) << 24) + (MASK(v0, 6) >> 8) +
                (MASK(v1, 0) << 48) + MASK(v0, 7);
    }

    private static void Permute(ulong[] v, ulong[] permuted)
    {
        permuted[0] = RotateRight(v[2], 0);
        permuted[1] = RotateRight(v[3], 0);
        permuted[2] = RotateRight(v[0], 0);
        permuted[3] = RotateRight(v[1], 0);
    }

    private static void PermuteAndUpdate()
    {
        ulong[] permuted = new ulong[4];
        Permute(v0, permuted);
        Update(permuted);
    }
}