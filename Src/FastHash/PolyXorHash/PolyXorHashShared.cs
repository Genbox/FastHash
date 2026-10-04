#if NET10_0_OR_GREATER

// C# port of orlp/polyxor 0.1.0 (3123eb6). Copyright (c) 2026 Orson Peters.
// Distributed under the zlib license; see LICENSE.txt in this directory.
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using Genbox.FastHash.Misc;
using ArmAes = System.Runtime.Intrinsics.Arm.Aes;

namespace Genbox.FastHash.PolyXorHash;

internal static class PolyXorHashShared
{
    internal static bool IsSupported => Pclmulqdq.IsSupported || (AdvSimd.Arm64.IsSupported && ArmAes.IsSupported);

    internal static void RequireSupported()
    {
        if (!IsSupported)
            throw new PlatformNotSupportedException("PolyXOR requires x86 PCLMULQDQ or ARM64 PMULL hardware support.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static UInt128 ToUInt128(Vector128<ulong> v) => new UInt128(v.GetElement(1), v.GetElement(0));

    internal static void PrepareIndex(PolyXorHashParams parameters, out Vector128<ulong> h0,
                                      out Vector128<ulong> h1, out Vector128<ulong> multipliers)
    {
        h0 = h1 = multipliers = Vector128<ulong>.Zero;
        if (!IsSupported)
            return;

        // C#-specific optimization: upstream Rust physically zero-pads messages
        // of 1-8 bytes to 128 bytes and uses general block compression.
        // For 1-8 bytes only r0's low word changes. Cache the zero-block compression
        // and the two carryless multipliers describing its input-dependent delta.
        // GF(2) linearity makes this exactly equivalent to compressing the padded
        // block; the polynomial recurrence, byte count, and output modes are unchanged.
        ref byte key = ref MemoryMarshal.GetArrayDataReference(parameters.BlockKey);
        Vector128<ulong> r0 = Load128(ref key, 0);
        Vector128<ulong> r1 = Load128(ref key, 16);
        Vector128<ulong> r2 = Load128(ref key, 32);
        Vector128<ulong> r3 = Load128(ref key, 48);
        Vector128<ulong> r4 = Load128(ref key, 64);
        Vector128<ulong> r5 = Load128(ref key, 80);
        Vector128<ulong> r6 = Load128(ref key, 96);
        Vector128<ulong> r7 = Load128(ref key, 112);
        Vector128<ulong> e0 = r0 ^ r1 ^ r4 ^ r5;
        Vector128<ulong> e1 = r2 ^ r3 ^ r6 ^ r7;
        CombineHashes(PairProduct(r0, r6), PairProduct(r1, r7), PairProduct(r4, r2),
            PairProduct(r5, r3), PairProduct(e0, e1), out h0, out h1);
        h1 ^= parameters.PolyY;
        multipliers = Vector128.Create(r6.GetElement(0), e1.GetElement(0));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<ulong> HashIndexRaw(ulong input, PolyXorHashParams parameters) =>
        Multiply(AbsorbShort(input, parameters.PolyZ, parameters) ^ parameters.PolyU, Vector128.Create(8UL, 0UL) ^ parameters.PolyY);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<ulong> HashShortRaw(ReadOnlySpan<byte> data, PolyXorHashParams parameters)
    {
        Debug.Assert(data.Length <= sizeof(ulong));
        Vector128<ulong> accum = parameters.PolyZ;
        if (!data.IsEmpty)
            accum = AbsorbShort(ReadShort(data), accum, parameters);

        // Empty input must skip block absorption, exactly as upstream does.
        return Multiply(accum ^ parameters.PolyU, Vector128.Create((ulong)data.Length, 0UL) ^ parameters.PolyY);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong ReadShort(ReadOnlySpan<byte> data)
    {
        Debug.Assert(data.Length is >= 1 and <= sizeof(ulong));
        int length = data.Length;
        if (length == sizeof(ulong))
            return Utilities.Read64(data);

        if (length >= sizeof(uint))
        {
            // The overlapping reads stay entirely within the message. OR keeps
            // the shared bytes intact and places the final bytes in their LE positions.
            ulong first = Utilities.Read32(data);
            ulong last = Utilities.Read32(data, length - sizeof(uint));
            return first | (last << ((length - sizeof(uint)) * 8));
        }

        if (length >= sizeof(ushort))
        {
            ulong value = Utilities.Read16(data, 0);
            if (length == 3)
                value |= (ulong)data[2] << 16;
            return value;
        }

        return data[0];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<ulong> AbsorbShort(ulong input, Vector128<ulong> accum, PolyXorHashParams parameters)
    {
        Vector128<ulong> deltaH0;
        Vector128<ulong> deltaH1;

        if (Pclmulqdq.IsSupported)
        {
            Vector128<ulong> value = Vector128.Create(input, 0UL);
            deltaH0 = Pclmulqdq.CarrylessMultiply(value, parameters.IndexMultipliers, 0x00);
            deltaH1 = Pclmulqdq.CarrylessMultiply(value, parameters.IndexMultipliers, 0x10);
        }
        else
        {
            Vector64<ulong> value = Vector64.Create(input);
            deltaH0 = ArmAes.PolynomialMultiplyWideningLower(value, parameters.IndexMultipliers.GetLower());
            deltaH1 = ArmAes.PolynomialMultiplyWideningLower(value, parameters.IndexMultipliers.GetUpper());
        }

        return Multiply(accum ^ parameters.PolyU, parameters.IndexH1 ^ deltaH1) ^ parameters.IndexH0 ^ deltaH0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<ulong> Multiply(Vector128<ulong> a, Vector128<ulong> b) =>
        Pclmulqdq.IsSupported ? MultiplyX86(a, b) : MultiplyArm(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<ulong> MultiplyX86(Vector128<ulong> a, Vector128<ulong> b)
    {
        Vector128<ulong> modulus = Vector128.Create(0xc200000000000000UL, 0UL);
        Vector128<ulong> ll = Pclmulqdq.CarrylessMultiply(a, b, 0x00);
        Vector128<ulong> hh = Pclmulqdq.CarrylessMultiply(a, b, 0x11);
        Vector128<ulong> aSum = a ^ Sse2.ShiftRightLogical128BitLane(a.AsByte(), 8).AsUInt64();
        Vector128<ulong> bSum = b ^ Sse2.ShiftRightLogical128BitLane(b.AsByte(), 8).AsUInt64();
        Vector128<ulong> mid = Pclmulqdq.CarrylessMultiply(aSum, bSum, 0x00) ^ ll ^ hh;
        Vector128<ulong> lo = ll ^ Sse2.ShiftLeftLogical128BitLane(mid.AsByte(), 8).AsUInt64();
        Vector128<ulong> hi = hh ^ Sse2.ShiftRightLogical128BitLane(mid.AsByte(), 8).AsUInt64();
        Vector128<ulong> foldLo = Pclmulqdq.CarrylessMultiply(lo, modulus, 0x00);
        lo ^= Sse2.Shuffle(foldLo.AsUInt32(), 0x4e).AsUInt64();
        Vector128<ulong> foldHi = Pclmulqdq.CarrylessMultiply(lo, modulus, 0x01);
        return foldHi ^ lo ^ hi;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<ulong> MultiplyArm(Vector128<ulong> a, Vector128<ulong> b)
    {
        Vector128<ulong> zero = Vector128<ulong>.Zero;
        Vector128<ulong> modulus = Vector128.Create(0xc200000000000000UL);
        Vector128<ulong> ll = ArmAes.PolynomialMultiplyWideningLower(a.GetLower(), b.GetLower());
        Vector128<ulong> hh = ArmAes.PolynomialMultiplyWideningUpper(a, b);
        Vector128<ulong> aSum = a ^ AdvSimd.ExtractVector128(a, a, 1);
        Vector128<ulong> bSum = b ^ AdvSimd.ExtractVector128(b, b, 1);
        Vector128<ulong> mid = ArmAes.PolynomialMultiplyWideningLower(aSum.GetLower(), bSum.GetLower()) ^ ll ^ hh;
        Vector128<ulong> lo = ll ^ AdvSimd.ExtractVector128(zero, mid, 1);
        Vector128<ulong> hi = hh ^ AdvSimd.ExtractVector128(mid, zero, 1);
        Vector128<ulong> foldLo = ArmAes.PolynomialMultiplyWideningLower(lo.GetLower(), modulus.GetLower());
        lo ^= AdvSimd.ExtractVector128(foldLo, foldLo, 1);
        return ArmAes.PolynomialMultiplyWideningUpper(lo, modulus) ^ lo ^ hi;
    }

    internal static Vector128<ulong> Avalanche(Vector128<ulong> raw, PolyXorHashParams parameters, ulong tweak)
    {
        ulong lo = raw.GetElement(0) + tweak;
        ulong hi = raw.GetElement(1);
        const ulong mul = 0x0e9846af9b1a615d;
        lo ^= lo >> 32;
        lo *= mul;
        hi ^= hi >> 32;
        hi *= mul;
        lo ^= BitOperations.RotateLeft(hi, 32);
        hi += lo;
        lo ^= lo >> 32;
        lo *= mul;
        hi ^= hi >> 32;
        hi *= mul;

        // Upstream deliberately places the mixed lo in the HIGH word here.
        return Multiply(Vector128.Create(hi, lo), parameters.AvalancheMul);
    }

    internal static void HashBlocks(ReadOnlySpan<byte> data, PolyXorHashParams parameters, ref Vector128<ulong> accum)
    {
        if (Avx512F.IsSupported && Avx512F.VL.IsSupported && Pclmulqdq.V512.IsSupported)
            PolyXorHashX86Shared.Hash512(data, parameters, ref accum);
        else if (Avx2.IsSupported && Pclmulqdq.V256.IsSupported)
            PolyXorHashX86Shared.Hash256(data, parameters, ref accum);
        else
            Hash128(data, parameters, ref accum);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [SkipLocalsInit]
    internal static void HashTail(ReadOnlySpan<byte> data, PolyXorHashParams parameters, ref Vector128<ulong> accum)
    {
        if (data.Length <= sizeof(ulong))
        {
            // A short tail after full 4 KiB chunks uses the same first-block key
            // as a short standalone message, so the C# shortcut applies here too.
            if (!data.IsEmpty)
                accum = AbsorbShort(ReadShort(data), accum, parameters);
            return;
        }

        int padded = (data.Length + 127) & ~127;

        // Unlike Rust's fixed 4 KiB sponge, this one-shot temporary stores only
        // the tail rounded to 128 bytes. Its bytes and compression are identical
        // to upstream. Keep the variable stack allocation out of the full-chunk path.
        Span<byte> tail = stackalloc byte[padded];
        data.CopyTo(tail);
        tail.Slice(data.Length, padded - data.Length).Clear();
        HashBlocks(tail, parameters, ref accum);
    }

    // The AVX/PCLMUL and NEON/PMULL backends have identical block layouts. The
    // architecture checks in the inlined helpers are JIT constants, not loop dispatch.
    // .NET 10 has no fixed-width NEON EOR3 intrinsic for upstream's optional
    // SHA3 optimization. This is the upstream baseline (non-SHA3) NEON path.
    internal static void Hash128(ReadOnlySpan<byte> data, PolyXorHashParams parameters, ref Vector128<ulong> accum)
    {
        ref byte source = ref MemoryMarshal.GetReference(data);
        ref byte key = ref MemoryMarshal.GetArrayDataReference(parameters.BlockKey);
        Vector128<ulong> p = accum;
        int offset = 0;

        while (offset < data.Length)
        {
            int size = Math.Min(4096, data.Length - offset);
            Vector128<ulong> ha = Vector128<ulong>.Zero, hb = ha, hc = ha, hd = ha, he = ha;

            for (int i = 0; i < size; i += 128)
            {
                nuint d = (nuint)(offset + i);
                nuint k = (nuint)i;
                Vector128<ulong> r0 = Load128(ref source, d) ^ Load128(ref key, k);
                Vector128<ulong> r1 = Load128(ref source, d + 16) ^ Load128(ref key, k + 16);
                Vector128<ulong> r2 = Load128(ref source, d + 32) ^ Load128(ref key, k + 32);
                Vector128<ulong> r3 = Load128(ref source, d + 48) ^ Load128(ref key, k + 48);
                Vector128<ulong> r4 = Load128(ref source, d + 64) ^ Load128(ref key, k + 64);
                Vector128<ulong> r5 = Load128(ref source, d + 80) ^ Load128(ref key, k + 80);
                Vector128<ulong> r6 = Load128(ref source, d + 96) ^ Load128(ref key, k + 96);
                Vector128<ulong> r7 = Load128(ref source, d + 112) ^ Load128(ref key, k + 112);
                Vector128<ulong> e0 = r0 ^ r1 ^ r4 ^ r5;
                Vector128<ulong> e1 = r2 ^ r3 ^ r6 ^ r7;
                ha ^= PairProduct(r0, r6);
                hb ^= PairProduct(r1, r7);
                hc ^= PairProduct(r4, r2);
                hd ^= PairProduct(r5, r3);
                he ^= PairProduct(e0, e1);
            }

            p = Combine(ha, hb, hc, hd, he, p, parameters);
            offset += size;
        }

        accum = p;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ulong> Load128(ref byte data, nuint offset) => Vector128.LoadUnsafe(ref data, offset).AsUInt64();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<ulong> PairProduct(Vector128<ulong> a, Vector128<ulong> b)
    {
        if (Pclmulqdq.IsSupported)
            return Pclmulqdq.CarrylessMultiply(a, b, 0x00) ^ Pclmulqdq.CarrylessMultiply(a, b, 0x11);
        return ArmAes.PolynomialMultiplyWideningLower(a.GetLower(), b.GetLower()) ^ ArmAes.PolynomialMultiplyWideningUpper(a, b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<ulong> Combine(Vector128<ulong> ha, Vector128<ulong> hb, Vector128<ulong> hc,
                                             Vector128<ulong> hd, Vector128<ulong> he, Vector128<ulong> p, PolyXorHashParams parameters)
    {
        CombineHashes(ha, hb, hc, hd, he, out Vector128<ulong> h0, out Vector128<ulong> h1);
        return Multiply(p ^ parameters.PolyU, h1 ^ parameters.PolyY) ^ h0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CombineHashes(Vector128<ulong> ha, Vector128<ulong> hb, Vector128<ulong> hc,
                                      Vector128<ulong> hd, Vector128<ulong> he, out Vector128<ulong> h0, out Vector128<ulong> h1)
    {
        Vector128<ulong> hcd = hc ^ hd;
        Vector128<ulong> swapped = Pclmulqdq.IsSupported
            ? Sse2.Shuffle(hcd.AsUInt32(), 0xb1).AsUInt64()
            : AdvSimd.ReverseElement32(hcd);
        Vector128<ulong> whcd = swapped ^ (hcd & Vector128.Create(0xffffffff00000000UL));
        h0 = ha ^ hb ^ hcd;
        h1 = hb ^ hd ^ he ^ whcd;
    }
}
#endif