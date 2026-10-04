#if NET10_0_OR_GREATER

// C# port of orlp/polyxor 0.1.0 (3123eb6). Copyright (c) 2026 Orson Peters.
// Distributed under the zlib license; see LICENSE.txt in this directory.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Genbox.FastHash.PolyXorHash;

internal static class PolyXorHashX86Shared
{
    internal static void Hash256(ReadOnlySpan<byte> data, PolyXorHashParams parameters, ref Vector128<ulong> accum)
    {
        ref byte source = ref MemoryMarshal.GetReference(data);
        ref byte key = ref MemoryMarshal.GetArrayDataReference(parameters.BlockKey);
        Vector128<ulong> p = accum;
        int offset = 0;

        while (offset < data.Length)
        {
            int size = Math.Min(4096, data.Length - offset);
            Vector256<ulong> ab = Vector256<ulong>.Zero, cd = ab, e = ab;
            int i = 0;

            for (; i + 256 <= size; i += 256)
            {
                nuint d = (nuint)(offset + i), k = (nuint)i;
                Vector256<ulong> y0 = Load256(ref source, d) ^ Load256(ref key, k);
                Vector256<ulong> y1 = Load256(ref source, d + 32) ^ Load256(ref key, k + 32);
                Vector256<ulong> y2 = Load256(ref source, d + 64) ^ Load256(ref key, k + 64);
                Vector256<ulong> y3 = Load256(ref source, d + 96) ^ Load256(ref key, k + 96);
                Vector256<ulong> y4 = Load256(ref source, d + 128) ^ Load256(ref key, k + 128);
                Vector256<ulong> y5 = Load256(ref source, d + 160) ^ Load256(ref key, k + 160);
                Vector256<ulong> y6 = Load256(ref source, d + 192) ^ Load256(ref key, k + 192);
                Vector256<ulong> y7 = Load256(ref source, d + 224) ^ Load256(ref key, k + 224);
                ab ^= Product256(y0, y3) ^ Product256(y4, y7);
                cd ^= Product256(y2, y1) ^ Product256(y6, y5);
                Vector256<ulong> e00 = y0 ^ y2, e10 = y1 ^ y3;
                Vector256<ulong> e01 = y4 ^ y6, e11 = y5 ^ y7;
                Vector256<ulong> e0 = Avx2.Blend(e00.AsUInt32(), e01.AsUInt32(), 0xf0).AsUInt64() ^ Avx2.Permute2x128(e00, e01, 0x21);
                Vector256<ulong> e1 = Avx2.Blend(e10.AsUInt32(), e11.AsUInt32(), 0xf0).AsUInt64() ^ Avx2.Permute2x128(e10, e11, 0x21);
                e ^= Product256(e0, e1);
            }

            if (i < size)
            {
                nuint d = (nuint)(offset + i), k = (nuint)i;
                Vector256<ulong> y0 = Load256(ref source, d) ^ Load256(ref key, k);
                Vector256<ulong> y1 = Load256(ref source, d + 32) ^ Load256(ref key, k + 32);
                Vector256<ulong> y2 = Load256(ref source, d + 64) ^ Load256(ref key, k + 64);
                Vector256<ulong> y3 = Load256(ref source, d + 96) ^ Load256(ref key, k + 96);
                ab ^= Product256(y0, y3);
                cd ^= Product256(y2, y1);
                Vector256<ulong> e00 = y0 ^ y2, e10 = y1 ^ y3;
                Vector256<ulong> zero = Vector256<ulong>.Zero;
                Vector256<ulong> e0 = Avx2.Blend(e00.AsUInt32(), zero.AsUInt32(), 0xf0).AsUInt64() ^ Avx2.Permute2x128(e00, zero, 0x21);
                Vector256<ulong> e1 = Avx2.Blend(e10.AsUInt32(), zero.AsUInt32(), 0xf0).AsUInt64() ^ Avx2.Permute2x128(e10, zero, 0x21);
                e ^= Product256(e0, e1);
            }

            p = PolyXorHashShared.Combine(ab.GetLower(), ab.GetUpper(), cd.GetLower(), cd.GetUpper(), e.GetLower() ^ e.GetUpper(), p, parameters);
            offset += size;
        }

        accum = p;
    }

    internal static void Hash512(ReadOnlySpan<byte> data, PolyXorHashParams parameters, ref Vector128<ulong> accum)
    {
        ref byte source = ref MemoryMarshal.GetReference(data);
        ref byte key = ref MemoryMarshal.GetArrayDataReference(parameters.BlockKey);
        Vector128<ulong> p = accum;
        Vector512<ulong> idxLo = Vector512.Create(0UL, 4UL, 1UL, 5UL, 8UL, 12UL, 9UL, 13UL);
        Vector512<ulong> idxHi = Vector512.Create(2UL, 6UL, 3UL, 7UL, 10UL, 14UL, 11UL, 15UL);
        int offset = 0;

        while (offset < data.Length)
        {
            int size = Math.Min(4096, data.Length - offset);
            Vector512<ulong> acc0 = Vector512<ulong>.Zero, acc1 = acc0, e = acc0;
            int i = 0;

            for (; i + 256 <= size; i += 256)
            {
                nuint d = (nuint)(offset + i), k = (nuint)i;
                Vector512<ulong> z0 = Load512(ref source, d) ^ Load512(ref key, k);
                Vector512<ulong> z1 = Load512(ref source, d + 64) ^ Load512(ref key, k + 64);
                Vector512<ulong> z2 = Load512(ref source, d + 128) ^ Load512(ref key, k + 128);
                Vector512<ulong> z3 = Load512(ref source, d + 192) ^ Load512(ref key, k + 192);
                Vector512<ulong> z1s = Avx512F.Shuffle4x128(z1, z1, 0x4e);
                Vector512<ulong> z3s = Avx512F.Shuffle4x128(z3, z3, 0x4e);
                acc0 = Avx512F.TernaryLogic(acc0, Pclmulqdq.V512.CarrylessMultiply(z0, z1s, 0x00), Pclmulqdq.V512.CarrylessMultiply(z0, z1s, 0x11), 0x96);
                acc1 = Avx512F.TernaryLogic(acc1, Pclmulqdq.V512.CarrylessMultiply(z2, z3s, 0x00), Pclmulqdq.V512.CarrylessMultiply(z2, z3s, 0x11), 0x96);
                Vector512<ulong> x01 = z0 ^ z1, x23 = z2 ^ z3;
                Vector512<ulong> x = Avx512F.PermuteVar8x64x2(x01, idxLo, x23) ^ Avx512F.PermuteVar8x64x2(x01, idxHi, x23);
                e ^= Pclmulqdq.V512.CarrylessMultiply(x, x, 0x01);
            }

            if (i < size)
            {
                nuint d = (nuint)(offset + i), k = (nuint)i;
                Vector512<ulong> z0 = Load512(ref source, d) ^ Load512(ref key, k);
                Vector512<ulong> z1 = Load512(ref source, d + 64) ^ Load512(ref key, k + 64);
                Vector512<ulong> z1s = Avx512F.Shuffle4x128(z1, z1, 0x4e);
                acc0 = Avx512F.TernaryLogic(acc0, Pclmulqdq.V512.CarrylessMultiply(z0, z1s, 0x00), Pclmulqdq.V512.CarrylessMultiply(z0, z1s, 0x11), 0x96);
                Vector512<ulong> x01 = z0 ^ z1;
                Vector512<ulong> x = Avx512F.PermuteVar8x64x2(x01, idxLo, Vector512<ulong>.Zero) ^ Avx512F.PermuteVar8x64x2(x01, idxHi, Vector512<ulong>.Zero);
                e ^= Pclmulqdq.V512.CarrylessMultiply(x, x, 0x01);
            }

            Vector512<ulong> acc = acc0 ^ acc1;
            Vector256<ulong> low = acc.GetLower(), high = acc.GetUpper();
            Vector256<ulong> e256 = e.GetLower() ^ e.GetUpper();
            p = PolyXorHashShared.Combine(low.GetLower(), low.GetUpper(), high.GetLower(), high.GetUpper(), e256.GetLower() ^ e256.GetUpper(), p, parameters);
            offset += size;
        }

        accum = p;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ulong> Load256(ref byte data, nuint offset) => Vector256.LoadUnsafe(ref data, offset).AsUInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector512<ulong> Load512(ref byte data, nuint offset) => Vector512.LoadUnsafe(ref data, offset).AsUInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<ulong> Product256(Vector256<ulong> a, Vector256<ulong> b) =>
        Pclmulqdq.V256.CarrylessMultiply(a, b, 0x00) ^ Pclmulqdq.V256.CarrylessMultiply(a, b, 0x11);
}
#endif