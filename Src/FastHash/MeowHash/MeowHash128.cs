#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Genbox.FastHash.MeowHash;

public static class MeowHash128
{
    public static bool IsSupported => Aes.IsSupported && Sse.IsSupported && Sse2.IsSupported && Ssse3.IsSupported;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("MeowHash requires AES, SSE, SSE2, and SSSE3 intrinsics.");

        Vector128<byte> res = HashLen8(input);
        return Unsafe.As<Vector128<byte>, UInt128>(ref res);
    }

    public static UInt128 ComputeHash(ReadOnlySpan<byte> data)
    {
        Vector128<byte> res = ComputeHashVector(data);
        return Unsafe.As<Vector128<byte>, UInt128>(ref res);
    }

    internal static Vector128<byte> ComputeHashVector(ReadOnlySpan<byte> data)
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("MeowHash requires AES, SSE, SSE2, and SSSE3 intrinsics.");

        return MeowHash(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> HashLen8(ulong input)
    {
        Vector128<byte> xmm0 = Vector128.Create(0x8d305a88a8f64332UL, 0x340737e0a2983131UL).AsByte();
        Vector128<byte> xmm1 = Vector128.Create(0x1df399228293404aUL, 0xc8e6c48ea9ef8200UL).AsByte();
        Vector128<byte> xmm2 = Vector128.Create(0x37018d631e825294UL, 0xc6904ef36c46e57bUL).AsByte();
        Vector128<byte> xmm3 = Vector128.Create(0x0dc5977c9bc20accUL, 0x9170545b5b4df8d3UL).AsByte();
        Vector128<byte> xmm4 = Vector128.Create(0xb19f97985d6d2179UL, 0x5afb8d69ba1013bdUL).AsByte();
        Vector128<byte> xmm5 = Vector128.Create(0xfbad01bd2dd7ffc2UL, 0xe967a2d6fe1a8e7bUL).AsByte();
        Vector128<byte> xmm6 = Vector128.Create(0xf9c7125f04c9a76bUL, 0xcf16397b94194a92UL).AsByte();
        Vector128<byte> xmm7 = Vector128.Create(0xc1ef58282e1f8070UL, 0xe67415870d923666UL).AsByte();

        Vector128<byte> xmm9 = Vector128.Create(input, 0UL).AsByte();
        Vector128<byte> xmm11 = Vector128<byte>.Zero;

        Vector128<byte> xmm8 = xmm9;
        Vector128<byte> xmm10 = xmm9;
        palignr(ref xmm8, xmm11, 15);
        palignr(ref xmm10, xmm11, 1);

        Vector128<byte> xmm12 = Vector128<byte>.Zero;
        Vector128<byte> xmm13 = Vector128<byte>.Zero;
        Vector128<byte> xmm14 = Vector128<byte>.Zero;
        movq(out Vector128<byte> xmm15, 8UL);
        palignr(ref xmm12, xmm15, 15);
        palignr(ref xmm14, xmm15, 1);

        MEOW_MIX_REG(ref xmm0, ref xmm4, ref xmm6, ref xmm1, ref xmm2, xmm8, xmm9, xmm10, xmm11);
        MEOW_MIX_REG(ref xmm1, ref xmm5, ref xmm7, ref xmm2, ref xmm3, xmm12, xmm13, xmm14, xmm15);

        return MixDown(xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);
    }

    private static Vector128<byte> MeowHash(ReadOnlySpan<byte> source)
    {
        int len = source.Length;

        Vector128<byte> xmm0 = Vector128.Create(0x8d305a88a8f64332UL, 0x340737e0a2983131UL).AsByte();
        Vector128<byte> xmm1 = Vector128.Create(0x1df399228293404aUL, 0xc8e6c48ea9ef8200UL).AsByte();
        Vector128<byte> xmm2 = Vector128.Create(0x37018d631e825294UL, 0xc6904ef36c46e57bUL).AsByte();
        Vector128<byte> xmm3 = Vector128.Create(0x0dc5977c9bc20accUL, 0x9170545b5b4df8d3UL).AsByte();
        Vector128<byte> xmm4 = Vector128.Create(0xb19f97985d6d2179UL, 0x5afb8d69ba1013bdUL).AsByte();
        Vector128<byte> xmm5 = Vector128.Create(0xfbad01bd2dd7ffc2UL, 0xe967a2d6fe1a8e7bUL).AsByte();
        Vector128<byte> xmm6 = Vector128.Create(0xf9c7125f04c9a76bUL, 0xcf16397b94194a92UL).AsByte();
        Vector128<byte> xmm7 = Vector128.Create(0xc1ef58282e1f8070UL, 0xe67415870d923666UL).AsByte();

        int offset = 0;
        int blockCount = len >> 8;

        while (blockCount-- > 0)
        {
            MEOW_MIX(ref xmm0, ref xmm4, ref xmm6, ref xmm1, ref xmm2, source, offset + 0x00);
            MEOW_MIX(ref xmm1, ref xmm5, ref xmm7, ref xmm2, ref xmm3, source, offset + 0x20);
            MEOW_MIX(ref xmm2, ref xmm6, ref xmm0, ref xmm3, ref xmm4, source, offset + 0x40);
            MEOW_MIX(ref xmm3, ref xmm7, ref xmm1, ref xmm4, ref xmm5, source, offset + 0x60);
            MEOW_MIX(ref xmm4, ref xmm0, ref xmm2, ref xmm5, ref xmm6, source, offset + 0x80);
            MEOW_MIX(ref xmm5, ref xmm1, ref xmm3, ref xmm6, ref xmm7, source, offset + 0xa0);
            MEOW_MIX(ref xmm6, ref xmm2, ref xmm4, ref xmm7, ref xmm0, source, offset + 0xc0);
            MEOW_MIX(ref xmm7, ref xmm3, ref xmm5, ref xmm0, ref xmm1, source, offset + 0xe0);

            offset += 0x100;
        }

        Span<byte> residual = stackalloc byte[32];
        int residualLength = len & 31;
        if (residualLength != 0)
            source.Slice(len - residualLength, residualLength).CopyTo(residual);

        Vector128<byte> xmm9 = LoadVector(residual, 0);
        Vector128<byte> xmm11 = LoadVector(residual, 16);

        Vector128<byte> xmm8 = xmm9;
        Vector128<byte> xmm10 = xmm9;
        palignr(ref xmm8, xmm11, 15);
        palignr(ref xmm10, xmm11, 1);

        Vector128<byte> xmm12 = Vector128<byte>.Zero;
        Vector128<byte> xmm13 = Vector128<byte>.Zero;
        Vector128<byte> xmm14 = Vector128<byte>.Zero;
        movq(out Vector128<byte> xmm15, (ulong)len);
        palignr(ref xmm12, xmm15, 15);
        palignr(ref xmm14, xmm15, 1);

        MEOW_MIX_REG(ref xmm0, ref xmm4, ref xmm6, ref xmm1, ref xmm2, xmm8, xmm9, xmm10, xmm11);
        MEOW_MIX_REG(ref xmm1, ref xmm5, ref xmm7, ref xmm2, ref xmm3, xmm12, xmm13, xmm14, xmm15);

        int laneCount = (len >> 5) & 0x7;

        if (laneCount == 0) return MixDown(xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);
        MEOW_MIX(ref xmm2, ref xmm6, ref xmm0, ref xmm3, ref xmm4, source, offset + 0x00);
        --laneCount;
        if (laneCount == 0) return MixDown(xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);
        MEOW_MIX(ref xmm3, ref xmm7, ref xmm1, ref xmm4, ref xmm5, source, offset + 0x20);
        --laneCount;
        if (laneCount == 0) return MixDown(xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);
        MEOW_MIX(ref xmm4, ref xmm0, ref xmm2, ref xmm5, ref xmm6, source, offset + 0x40);
        --laneCount;
        if (laneCount == 0) return MixDown(xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);
        MEOW_MIX(ref xmm5, ref xmm1, ref xmm3, ref xmm6, ref xmm7, source, offset + 0x60);
        --laneCount;
        if (laneCount == 0) return MixDown(xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);
        MEOW_MIX(ref xmm6, ref xmm2, ref xmm4, ref xmm7, ref xmm0, source, offset + 0x80);
        --laneCount;
        if (laneCount == 0) return MixDown(xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);
        MEOW_MIX(ref xmm7, ref xmm3, ref xmm5, ref xmm0, ref xmm1, source, offset + 0xa0);
        --laneCount;
        if (laneCount != 0)
            MEOW_MIX(ref xmm0, ref xmm4, ref xmm6, ref xmm1, ref xmm2, source, offset + 0xc0);

        return MixDown(xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> MixDown(Vector128<byte> xmm0, Vector128<byte> xmm1, Vector128<byte> xmm2, Vector128<byte> xmm3, Vector128<byte> xmm4, Vector128<byte> xmm5, Vector128<byte> xmm6, Vector128<byte> xmm7)
    {
        MEOW_SHUFFLE(ref xmm0, ref xmm1, xmm2, ref xmm4, ref xmm5, xmm6);
        MEOW_SHUFFLE(ref xmm1, ref xmm2, xmm3, ref xmm5, ref xmm6, xmm7);
        MEOW_SHUFFLE(ref xmm2, ref xmm3, xmm4, ref xmm6, ref xmm7, xmm0);
        MEOW_SHUFFLE(ref xmm3, ref xmm4, xmm5, ref xmm7, ref xmm0, xmm1);
        MEOW_SHUFFLE(ref xmm4, ref xmm5, xmm6, ref xmm0, ref xmm1, xmm2);
        MEOW_SHUFFLE(ref xmm5, ref xmm6, xmm7, ref xmm1, ref xmm2, xmm3);
        MEOW_SHUFFLE(ref xmm6, ref xmm7, xmm0, ref xmm2, ref xmm3, xmm4);
        MEOW_SHUFFLE(ref xmm7, ref xmm0, xmm1, ref xmm3, ref xmm4, xmm5);
        MEOW_SHUFFLE(ref xmm0, ref xmm1, xmm2, ref xmm4, ref xmm5, xmm6);
        MEOW_SHUFFLE(ref xmm1, ref xmm2, xmm3, ref xmm5, ref xmm6, xmm7);
        MEOW_SHUFFLE(ref xmm2, ref xmm3, xmm4, ref xmm6, ref xmm7, xmm0);
        MEOW_SHUFFLE(ref xmm3, ref xmm4, xmm5, ref xmm7, ref xmm0, xmm1);

        paddq(ref xmm0, xmm2);
        paddq(ref xmm1, xmm3);
        paddq(ref xmm4, xmm6);
        paddq(ref xmm5, xmm7);
        pxor(ref xmm0, xmm1);
        pxor(ref xmm4, xmm5);
        paddq(ref xmm0, xmm4);

        return xmm0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MEOW_MIX_REG(ref Vector128<byte> r1, ref Vector128<byte> r2, ref Vector128<byte> r3, ref Vector128<byte> r4, ref Vector128<byte> r5, Vector128<byte> i1, Vector128<byte> i2, Vector128<byte> i3, Vector128<byte> i4)
    {
        aesdec(ref r1, r2);
        paddq(ref r3, i1);
        pxor(ref r2, i2);
        aesdec(ref r2, r4);
        paddq(ref r5, i3);
        pxor(ref r4, i4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MEOW_MIX(ref Vector128<byte> r1, ref Vector128<byte> r2, ref Vector128<byte> r3, ref Vector128<byte> r4, ref Vector128<byte> r5, ReadOnlySpan<byte> data, int offset)
    {
        MEOW_MIX_REG(ref r1, ref r2, ref r3, ref r4, ref r5,
            LoadVector(data, offset + 15),
            LoadVector(data, offset),
            LoadVector(data, offset + 1),
            LoadVector(data, offset + 16)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> LoadVector(ReadOnlySpan<byte> data, int offset)
    {
        ref byte ptr = ref MemoryMarshal.GetReference(data);
        return Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref ptr, offset));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MEOW_SHUFFLE(ref Vector128<byte> r1, ref Vector128<byte> r2, Vector128<byte> r3, ref Vector128<byte> r4, ref Vector128<byte> r5, Vector128<byte> r6)
    {
        aesdec(ref r1, r4);
        paddq(ref r2, r5);
        pxor(ref r4, r6);
        aesdec(ref r4, r2);
        paddq(ref r5, r6);
        pxor(ref r2, r3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void movq(out Vector128<byte> A, ulong B) => A = Vector128.Create(B, 0ul).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void aesdec(ref Vector128<byte> A, Vector128<byte> B) => A = Aes.Decrypt(A, B);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void pxor(ref Vector128<byte> A, Vector128<byte> B) => A = Sse2.Xor(A, B);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void paddq(ref Vector128<byte> A, Vector128<byte> B) => A = Sse2.Add(A.AsUInt64(), B.AsUInt64()).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void palignr(ref Vector128<byte> A, Vector128<byte> B, byte i) => A = Ssse3.AlignRight(A, B, i);
}
#endif