#if NET10_0_OR_GREATER

// C# port of orlp/polyxor 0.1.0 (3123eb6). Copyright (c) 2026 Orson Peters.
// Distributed under the zlib license; see LICENSE.txt in this directory.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

using System.Diagnostics;
using System.Numerics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using Genbox.FastHash.Misc;
using ArmAes = System.Runtime.Intrinsics.Arm.Aes;

namespace Genbox.FastHash.PolyXorHash;

/// <summary>Provides hardware-accelerated PolyXOR128 hashing.</summary>
/// <remarks>Raw and avalanched hashes do not hide the key. Use MAC output when authentication is required.</remarks>
public static class PolyXorHash128
{
    /// <summary>Gets whether hardware carryless multiplication is available.</summary>
    public static bool IsSupported => Pclmulqdq.IsSupported || (AdvSimd.Arm64.IsSupported && ArmAes.IsSupported);

    private static class DefaultParameters
    {
        internal static readonly PolyXorHashParams Value = new PolyXorHashParams(default(UInt128));
    }

    /// <summary>Computes an avalanched hash using the fixed all-zero key.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, DefaultParameters.Value);

    /// <summary>Computes an avalanched hash using parameters derived from a key.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="key">The 128-bit AES key used to derive the parameters.</param>
    /// <param name="tweak">An optional value mixed in before avalanching.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <remarks>Expands the key on each call. Reuse <see cref="PolyXorHashParams" /> when hashing multiple messages.</remarks>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">AES key expansion failed.</exception>
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, UInt128 key, ulong tweak = 0) =>
        ComputeHash(data, new PolyXorHashParams(key), tweak);

    /// <summary>Computes an avalanched hash using reusable parameters.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="parameters">The expanded key or entropy.</param>
    /// <param name="tweak">An optional value mixed in before avalanching.</param>
    /// <returns>The 128-bit hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, PolyXorHashParams parameters, ulong tweak = 0) =>
        PolyXorHash128.ToUInt128(PolyXorHash128.Avalanche(ComputeRaw(data, parameters), parameters, tweak));

    /// <summary>Computes the raw universal hash without avalanching or blinding.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="parameters">The expanded key or entropy.</param>
    /// <returns>The raw 128-bit hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    public static UInt128 ComputeHashRaw(ReadOnlySpan<byte> data, PolyXorHashParams parameters) =>
        PolyXorHash128.ToUInt128(ComputeRaw(data, parameters));

    /// <summary>Computes the raw universal hash using parameters derived from a key.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="key">The 128-bit AES key used to derive the parameters.</param>
    /// <returns>The raw 128-bit hash.</returns>
    /// <remarks>Expands the key on each call. Reuse <see cref="PolyXorHashParams" /> when hashing multiple messages.</remarks>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">AES key expansion failed.</exception>
    public static UInt128 ComputeHashRaw(ReadOnlySpan<byte> data, UInt128 key) => ComputeHashRaw(data, new PolyXorHashParams(key));

    /// <summary>Computes a nonce-based authentication tag.</summary>
    /// <param name="data">The bytes to authenticate.</param>
    /// <param name="parameters">Parameters expanded from an AES key.</param>
    /// <param name="nonce">The nonce; the top two bits are ignored.</param>
    /// <returns>The 128-bit tag.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="parameters" /> was initialized from entropy instead of an AES key.</exception>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication or AES intrinsics are unavailable.</exception>
    public static UInt128 ComputeMac(ReadOnlySpan<byte> data, PolyXorHashParams parameters, UInt128 nonce)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        return parameters.Mac(ComputeRaw(data, parameters), nonce);
    }

    /// <summary>Computes a nonce-based authentication tag using parameters derived from a key.</summary>
    /// <param name="data">The bytes to authenticate.</param>
    /// <param name="key">The 128-bit AES key.</param>
    /// <param name="nonce">The nonce; the top two bits are ignored.</param>
    /// <returns>The 128-bit tag.</returns>
    /// <remarks>Expands the key on each call. Reuse <see cref="PolyXorHashParams" /> when authenticating multiple messages.</remarks>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication or AES intrinsics are unavailable.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">AES key expansion failed.</exception>
    public static UInt128 ComputeMac(ReadOnlySpan<byte> data, UInt128 key, UInt128 nonce) => ComputeMac(data, new PolyXorHashParams(key), nonce);

    /// <summary>Hashes a 64-bit integer using the fixed all-zero key.</summary>
    /// <param name="input">The integer encoded in little-endian order.</param>
    /// <returns>The avalanched hash.</returns>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input) => ComputeIndex(input, DefaultParameters.Value);

    /// <summary>Hashes a 64-bit integer using parameters derived from a key.</summary>
    /// <param name="input">The integer encoded in little-endian order.</param>
    /// <param name="key">The 128-bit AES key used to derive the parameters.</param>
    /// <param name="tweak">The avalanche tweak.</param>
    /// <returns>The avalanched hash.</returns>
    /// <remarks>Expands the key on each call. Reuse <see cref="PolyXorHashParams" /> when hashing multiple integers.</remarks>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    /// <exception cref="System.Security.Cryptography.CryptographicException">AES key expansion failed.</exception>
    public static UInt128 ComputeIndex(ulong input, UInt128 key, ulong tweak = 0) => ComputeIndex(input, new PolyXorHashParams(key), tweak);

    /// <summary>Hashes a 64-bit integer using reusable parameters.</summary>
    /// <param name="input">The integer encoded in little-endian order.</param>
    /// <param name="parameters">The expanded parameters.</param>
    /// <param name="tweak">The avalanche tweak.</param>
    /// <returns>The avalanched hash.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    /// <exception cref="PlatformNotSupportedException">Required carryless multiplication intrinsics are unavailable.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ComputeIndex(ulong input, PolyXorHashParams parameters, ulong tweak = 0)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        PolyXorHash128.RequireSupported();
        Vector128<ulong> raw = PolyXorHash128.HashIndexRaw(input, parameters);
        return PolyXorHash128.ToUInt128(PolyXorHash128.Avalanche(raw, parameters, tweak));
    }

    private static Vector128<ulong> ComputeRaw(ReadOnlySpan<byte> data, PolyXorHashParams parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        PolyXorHash128.RequireSupported();
        if (data.Length <= sizeof(ulong))
            return PolyXorHash128.HashShortRaw(data, parameters);
        Vector128<ulong> accum = parameters.PolyZ;
        int whole = data.Length & ~4095;
        if (whole != 0)
            PolyXorHash128.HashBlocks(data[..whole], parameters, ref accum);
        if (whole != data.Length)
            PolyXorHash128.HashTail(data[whole..], parameters, ref accum);
        return PolyXorHash128.Multiply(accum ^ parameters.PolyU, Vector128.Create((ulong)data.Length, 0UL) ^ parameters.PolyY);
    }

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
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
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
                value |= (ulong)Unsafe.Add(ref dataReference, 2) << 16;
            return value;
        }

        return Unsafe.Add(ref dataReference, 0);
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
            X86.Hash512(data, parameters, ref accum);
        else if (Avx2.IsSupported && Pclmulqdq.V256.IsSupported)
            X86.Hash256(data, parameters, ref accum);
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

#if NET10_0_OR_GREATER
    internal static class X86
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

                p = PolyXorHash128.Combine(ab.GetLower(), ab.GetUpper(), cd.GetLower(), cd.GetUpper(), e.GetLower() ^ e.GetUpper(), p, parameters);
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
                p = PolyXorHash128.Combine(low.GetLower(), low.GetUpper(), high.GetLower(), high.GetUpper(), e256.GetLower() ^ e256.GetUpper(), p, parameters);
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
}
#endif