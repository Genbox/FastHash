#if NET8_0
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Genbox.FastHash.MeowHash;

public static class MeowHash128Unsafe
{
    private static readonly byte[] _shiftAdjust = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15];
    private static readonly byte[] _maskLen =
    [
        255, 255, 255, 255,
        255, 255, 255, 255,
        255, 255, 255, 255,
        255, 255, 255, 255,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0
    ];

    // NOTE(casey): The default seed is now a "nothing-up-our-sleeves" number for good measure.  You may verify that it is just an encoding of Pi.
    public static readonly byte[] _defaultSeed =
    [
        0x32, 0x43, 0xF6, 0xA8, 0x88, 0x5A, 0x30, 0x8D,
        0x31, 0x31, 0x98, 0xA2, 0xE0, 0x37, 0x07, 0x34,
        0x4A, 0x40, 0x93, 0x82, 0x22, 0x99, 0xF3, 0x1D,
        0x00, 0x82, 0xEF, 0xA9, 0x8E, 0xC4, 0xE6, 0xC8,
        0x94, 0x52, 0x82, 0x1E, 0x63, 0x8D, 0x01, 0x37,
        0x7B, 0xE5, 0x46, 0x6C, 0xF3, 0x4E, 0x90, 0xC6,
        0xCC, 0x0A, 0xC2, 0x9B, 0x7C, 0x97, 0xC5, 0x0D,
        0xD3, 0xF8, 0x4D, 0x5B, 0x5B, 0x54, 0x70, 0x91,
        0x79, 0x21, 0x6D, 0x5D, 0x98, 0x97, 0x9F, 0xB1,
        0xBD, 0x13, 0x10, 0xBA, 0x69, 0x8D, 0xFB, 0x5A,
        0xC2, 0xFF, 0xD7, 0x2D, 0xBD, 0x01, 0xAD, 0xFB,
        0x7B, 0x8E, 0x1A, 0xFE, 0xD6, 0xA2, 0x67, 0xE9,
        0x6B, 0xA7, 0xC9, 0x04, 0x5F, 0x12, 0xC7, 0xF9,
        0x92, 0x4A, 0x19, 0x94, 0x7B, 0x39, 0x16, 0xCF,
        0x70, 0x80, 0x1F, 0x2E, 0x28, 0x58, 0xEF, 0xC1,
        0x66, 0x36, 0x92, 0x0D, 0x87, 0x15, 0x74, 0xE6
    ];

    private const int MEOW_PREFETCH_LIMIT = 0x3ff;
    private const int MEOW_PREFETCH = 4096;
    private const int MEOW_PAGESIZE = 4096;

    public static unsafe UInt128 ComputeHash(byte* data, int len)
    {
        fixed (byte* seedPtr = _defaultSeed)
        {
            Vector128<byte> res = MeowHash(seedPtr, len, data);
            return Unsafe.As<Vector128<byte>, UInt128>(ref res);
        }
    }

    [SuppressMessage("Major Code Smell", "S907:\"goto\" statement should not be used")]
    public static unsafe Vector128<byte> MeowHash(byte* seed128Init, int len, byte* sourceInit)
    {
        Vector128<byte> xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7; // NOTE(casey): xmm0-xmm7 are the hash accumulation lanes
        Vector128<byte> xmm8, xmm9, xmm10, xmm11, xmm12, xmm13, xmm14, xmm15; // NOTE(casey): xmm8-xmm15 hold values to be appended (residual, length)

        byte* rax = sourceInit;
        byte* rcx = seed128Init;

        // NOTE(casey): Seed the eight hash registers
        movdqu(out xmm0, rcx + 0x00);
        movdqu(out xmm1, rcx + 0x10);
        movdqu(out xmm2, rcx + 0x20);
        movdqu(out xmm3, rcx + 0x30);

        movdqu(out xmm4, rcx + 0x40);
        movdqu(out xmm5, rcx + 0x50);
        movdqu(out xmm6, rcx + 0x60);
        movdqu(out xmm7, rcx + 0x70);

        MEOW_DUMP_STATE("Seed", xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);

        // NOTE(casey): Hash all full 256-byte blocks
        int blockCount = len >> 8;

        if (blockCount > MEOW_PREFETCH_LIMIT)
        {
            // NOTE(casey): For large input, modern Intel x64's can't hit full speed without prefetching, so we use this loop
            while (blockCount-- > 0)
            {
                prefetcht0(rax + MEOW_PREFETCH + 0x00);
                prefetcht0(rax + MEOW_PREFETCH + 0x40);
                prefetcht0(rax + MEOW_PREFETCH + 0x80);
                prefetcht0(rax + MEOW_PREFETCH + 0xc0);

                MEOW_MIX(ref xmm0, ref xmm4, ref xmm6, ref xmm1, ref xmm2, rax + 0x00);
                MEOW_MIX(ref xmm1, ref xmm5, ref xmm7, ref xmm2, ref xmm3, rax + 0x20);
                MEOW_MIX(ref xmm2, ref xmm6, ref xmm0, ref xmm3, ref xmm4, rax + 0x40);
                MEOW_MIX(ref xmm3, ref xmm7, ref xmm1, ref xmm4, ref xmm5, rax + 0x60);
                MEOW_MIX(ref xmm4, ref xmm0, ref xmm2, ref xmm5, ref xmm6, rax + 0x80);
                MEOW_MIX(ref xmm5, ref xmm1, ref xmm3, ref xmm6, ref xmm7, rax + 0xa0);
                MEOW_MIX(ref xmm6, ref xmm2, ref xmm4, ref xmm7, ref xmm0, rax + 0xc0);
                MEOW_MIX(ref xmm7, ref xmm3, ref xmm5, ref xmm0, ref xmm1, rax + 0xe0);

                rax += 0x100;
            }
        }
        else
        {
            // NOTE(casey): For small input, modern Intel x64's can't hit full speed _with_ prefetching (because of port pressure), so we use this loop.
            while (blockCount-- > 0)
            {
                MEOW_MIX(ref xmm0, ref xmm4, ref xmm6, ref xmm1, ref xmm2, rax + 0x00);
                MEOW_MIX(ref xmm1, ref xmm5, ref xmm7, ref xmm2, ref xmm3, rax + 0x20);
                MEOW_MIX(ref xmm2, ref xmm6, ref xmm0, ref xmm3, ref xmm4, rax + 0x40);
                MEOW_MIX(ref xmm3, ref xmm7, ref xmm1, ref xmm4, ref xmm5, rax + 0x60);
                MEOW_MIX(ref xmm4, ref xmm0, ref xmm2, ref xmm5, ref xmm6, rax + 0x80);
                MEOW_MIX(ref xmm5, ref xmm1, ref xmm3, ref xmm6, ref xmm7, rax + 0xa0);
                MEOW_MIX(ref xmm6, ref xmm2, ref xmm4, ref xmm7, ref xmm0, rax + 0xc0);
                MEOW_MIX(ref xmm7, ref xmm3, ref xmm5, ref xmm0, ref xmm1, rax + 0xe0);

                rax += 0x100;
            }
        }

        MEOW_DUMP_STATE("PostBlocks", xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);

        // NOTE(casey): Load any less-than-32-byte residual
        xmm9 = Vector128<byte>.Zero;
        xmm11 = Vector128<byte>.Zero;

        // TODO(casey): I need to put more thought into how the end-of-buffer stuff is actually working out here,
        // because I _think_ it may be possible to remove the first branch (on Len8) and let the mask zero out the
        // result, but it would take a little thought to make sure it couldn't read off the end of the buffer due
        // to the & 0xf on the align computation.

        // NOTE(casey): First, we have to load the part that is _not_ 16-byte aligned
        byte* last = sourceInit + (len & ~0xf);
        uint len8 = (uint)(len & 0xf);
        if (len8 > 0)
        {
            // NOTE(casey): Load the mask early
            fixed (byte* meowMaskLen = _maskLen)
            {
                xmm8 = Sse2.LoadVector128(&meowMaskLen[0x10 - len8]);
            }

            byte* lastOk = (byte*)(((ulong)(sourceInit + len - 1) | (MEOW_PAGESIZE - 1)) - 16);

            int align = last > lastOk ? (int)(ulong)last & 0xf : 0;

            fixed (byte* meowShiftAdjust = _shiftAdjust)
            {
                movdqu(out xmm10, &meowShiftAdjust[align]);
            }

            movdqu(out xmm9, last - align);
            pshufb(ref xmm9, xmm10);

            // NOTE(jeffr): and off the extra bytes
            pand(ref xmm9, xmm8);
        }

        // NOTE(casey): Next, we have to load the part that _is_ 16-byte aligned
        if ((len & 0x10) != 0)
        {
            xmm11 = xmm9;
            movdqu(out xmm9, last - 0x10);
        }

        // NOTE(casey): Construct the residual and length injests
        xmm8 = xmm9;
        xmm10 = xmm9;
        palignr(ref xmm8, xmm11, 15);
        palignr(ref xmm10, xmm11, 1);

        // NOTE(casey): We have room for a 128-bit nonce and a 64-bit none here, but
        // the decision was made to leave them zero'd so as not to confuse people
        // about hwo to use them or what security implications they had.
        xmm12 = Vector128<byte>.Zero;
        xmm13 = Vector128<byte>.Zero;
        xmm14 = Vector128<byte>.Zero;
        movq(out xmm15, (ulong)len);
        palignr(ref xmm12, xmm15, 15);
        palignr(ref xmm14, xmm15, 1);

        MEOW_DUMP_STATE("Residuals", xmm8, xmm9, xmm10, xmm11, xmm12, xmm13, xmm14, xmm15);

        // NOTE(casey): To maintain the mix-down pattern, we always Meow Mix the less-than-32-byte residual, even if it was empty
        MEOW_MIX_REG(ref xmm0, ref xmm4, ref xmm6, ref xmm1, ref xmm2, xmm8, xmm9, xmm10, xmm11);

        // NOTE(casey): Append the length, to avoid problems with our 32-byte padding
        MEOW_MIX_REG(ref xmm1, ref xmm5, ref xmm7, ref xmm2, ref xmm3, xmm12, xmm13, xmm14, xmm15);

        MEOW_DUMP_STATE("PostAppend", xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);

        // NOTE(casey): Hash all full 32-byte blocks
        int laneCount = (len >> 5) & 0x7;

        if (laneCount == 0) goto MixDown;
        MEOW_MIX(ref xmm2, ref xmm6, ref xmm0, ref xmm3, ref xmm4, rax + 0x00);
        --laneCount;
        if (laneCount == 0) goto MixDown;
        MEOW_MIX(ref xmm3, ref xmm7, ref xmm1, ref xmm4, ref xmm5, rax + 0x20);
        --laneCount;
        if (laneCount == 0) goto MixDown;
        MEOW_MIX(ref xmm4, ref xmm0, ref xmm2, ref xmm5, ref xmm6, rax + 0x40);
        --laneCount;
        if (laneCount == 0) goto MixDown;
        MEOW_MIX(ref xmm5, ref xmm1, ref xmm3, ref xmm6, ref xmm7, rax + 0x60);
        --laneCount;
        if (laneCount == 0) goto MixDown;
        MEOW_MIX(ref xmm6, ref xmm2, ref xmm4, ref xmm7, ref xmm0, rax + 0x80);
        --laneCount;
        if (laneCount == 0) goto MixDown;
        MEOW_MIX(ref xmm7, ref xmm3, ref xmm5, ref xmm0, ref xmm1, rax + 0xa0);
        --laneCount;
        if (laneCount == 0) goto MixDown;
        MEOW_MIX(ref xmm0, ref xmm4, ref xmm6, ref xmm1, ref xmm2, rax + 0xc0);

        // NOTE(casey): Mix the eight lanes down to one 128-bit hash

        MixDown:

        MEOW_DUMP_STATE("PostLanes", xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);

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

        MEOW_DUMP_STATE("PostMix", xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);

        paddq(ref xmm0, xmm2);
        paddq(ref xmm1, xmm3);
        paddq(ref xmm4, xmm6);
        paddq(ref xmm5, xmm7);
        pxor(ref xmm0, xmm1);
        pxor(ref xmm4, xmm5);
        paddq(ref xmm0, xmm4);

        MEOW_DUMP_STATE("PostFold", xmm0, xmm1, xmm2, xmm3, xmm4, xmm5, xmm6, xmm7);

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
    private static unsafe void MEOW_MIX(ref Vector128<byte> r1, ref Vector128<byte> r2, ref Vector128<byte> r3, ref Vector128<byte> r4, ref Vector128<byte> r5, byte* ptr)
    {
        MEOW_MIX_REG(ref r1, ref r2, ref r3, ref r4, ref r5,
            Sse2.LoadVector128(ptr + 15),
            Sse2.LoadVector128(ptr + 0),
            Sse2.LoadVector128(ptr + 1),
            Sse2.LoadVector128(ptr + 16)
        );
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
    private static unsafe void prefetcht0(void* addr) => Sse.Prefetch0(addr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void movdqu(out Vector128<byte> A, byte* B) => A = Sse2.LoadVector128(B);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void movq(out Vector128<byte> A, ulong B) => A = Vector128.Create(B, 0ul).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void aesdec(ref Vector128<byte> A, Vector128<byte> B) => A = Aes.Decrypt(A, B);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void pshufb(ref Vector128<byte> A, Vector128<byte> B) => A = Ssse3.Shuffle(A, B);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void pxor(ref Vector128<byte> A, Vector128<byte> B) => A = Sse2.Xor(A, B);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void paddq(ref Vector128<byte> A, Vector128<byte> B) => A = Sse2.Add(A.AsUInt64(), B.AsUInt64()).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void pand(ref Vector128<byte> A, Vector128<byte> B) => A = Sse2.Add(A, B);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void palignr(ref Vector128<byte> A, Vector128<byte> B, byte i) => A = Ssse3.AlignRight(A, B, i);

    [Conditional("MEOW_DEBUG")]
    private static void MEOW_DUMP_STATE(string title, Vector128<byte> xmm0, Vector128<byte> xmm1, Vector128<byte> xmm2, Vector128<byte> xmm3, Vector128<byte> xmm4, Vector128<byte> xmm5, Vector128<byte> xmm6, Vector128<byte> xmm7)
    {
        Console.Write(title + ": ");
        PrintVector(xmm0);
        PrintVector(xmm1);
        PrintVector(xmm2);
        PrintVector(xmm3);
        PrintVector(xmm4);
        PrintVector(xmm5);
        PrintVector(xmm6);
        PrintVector(xmm7);
        Console.WriteLine();
    }

    [Conditional("MEOW_DEBUG")]
    private static void MEOW_DUMP_STATE(string title, Vector128<byte> xmm0)
    {
        Console.Write(title + ": ");
        PrintVector(xmm0);
        Console.WriteLine();
    }

    [Conditional("MEOW_DEBUG")]
    private static void PrintVector(Vector128<byte> vector)
    {
        var values = vector.AsUInt64();

        for (int i = 0; i < 2; i++)
            Console.Write(values[i] + " ");
    }
}
#endif