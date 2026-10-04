// Test-only C# translation of orlp/polyxor's portable reference.rs (3123eb6).
// Copyright (c) 2026 Orson Peters. Zlib license: FastHash/PolyXorHash/LICENSE.txt.
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.Intrinsics;
using Genbox.FastHash.PolyXorHash;

namespace Genbox.FastHash.Tests.Single;

internal static class PolyXorReference
{
    internal static UInt128 Load(ReadOnlySpan<byte> data) =>
        ((UInt128)BinaryPrimitives.ReadUInt64LittleEndian(data[8..]) << 64) | BinaryPrimitives.ReadUInt64LittleEndian(data);

    private static UInt128 Clmul(ulong x, ulong y)
    {
        UInt128 result = 0;

        for (int i = 0; i < 64; i++)
        {
            result <<= 1;
            ulong mask = unchecked((ulong)((long)x >> 63));
            result ^= mask & y;
            x <<= 1;
        }

        return result;
    }

    internal static UInt128 Multiply(UInt128 a, UInt128 b)
    {
        unchecked
        {
            const ulong poly = 0xc200000000000000;
            ulong al = (ulong)a, ah = (ulong)(a >> 64), bl = (ulong)b, bh = (ulong)(b >> 64);
            UInt128 ll = Clmul(al, bl), hh = Clmul(ah, bh);
            UInt128 mid = Clmul(al ^ ah, bl ^ bh) ^ ll ^ hh;
            UInt128 lo = ll ^ (mid << 64), hi = hh ^ (mid >> 64);
            UInt128 foldLo = Clmul((ulong)lo, poly);
            lo ^= (foldLo << 64) | (foldLo >> 64);
            return Clmul((ulong)(lo >> 64), poly) ^ lo ^ hi;
        }
    }

    private static UInt128 Gf4(UInt128 x)
    {
        UInt128 low = ((UInt128)0xffffffff << 64) | 0xffffffff;
        return (((x & low) << 32) | ((x >> 32) & low)) ^ (x & ~low);
    }

    private static (UInt128 H0, UInt128 H1) Compress(ReadOnlySpan<ulong> m, int j)
    {
        UInt128 ha = Clmul(m[j], m[12 + j]);
        UInt128 hb = Clmul(m[2 + j], m[14 + j]);
        UInt128 hc = Clmul(m[8 + j], m[4 + j]);
        UInt128 hd = Clmul(m[10 + j], m[6 + j]);
        UInt128 he = Clmul(m[j] ^ m[2 + j] ^ m[8 + j] ^ m[10 + j], m[12 + j] ^ m[14 + j] ^ m[4 + j] ^ m[6 + j]);
        return (ha ^ hb ^ hc ^ hd, hb ^ Gf4(hc ^ hd) ^ hd ^ he);
    }

    internal static UInt128 HashBlocks(ReadOnlySpan<byte> data, PolyXorHashParams parameters, UInt128 accum)
    {
        Span<ulong> m = stackalloc ulong[16];
        UInt128 u = ((UInt128)parameters.PolyU.GetElement(1) << 64) | parameters.PolyU.GetElement(0);
        UInt128 y = ((UInt128)parameters.PolyY.GetElement(1) << 64) | parameters.PolyY.GetElement(0);

        while (data.Length >= 128)
        {
            int size = Math.Min(data.Length / 128, 32) * 128;
            UInt128 h0 = 0, h1 = 0;

            for (int i = 0; i < size; i += 128)
            {
                for (int j = 0; j < 16; j++)
                {
                    m[j] = BinaryPrimitives.ReadUInt64LittleEndian(data[(i + (8 * j))..]) ^
                           BinaryPrimitives.ReadUInt64LittleEndian(parameters.BlockKey.AsSpan(i + (8 * j)));
                }

                (UInt128 a0, UInt128 a1) = Compress(m, 0);
                (UInt128 b0, UInt128 b1) = Compress(m, 1);
                h0 ^= a0 ^ b0;
                h1 ^= a1 ^ b1;
            }

            accum = Multiply(accum ^ u, h1 ^ y) ^ h0;
            data = data[size..];
        }

        return accum;
    }

    internal static UInt128 Raw(ReadOnlySpan<byte> data, PolyXorHashParams parameters)
    {
        UInt128 accum = ((UInt128)parameters.PolyZ.GetElement(1) << 64) | parameters.PolyZ.GetElement(0);
        int whole = data.Length & ~4095;
        accum = HashBlocks(data[..whole], parameters, accum);

        if (whole != data.Length)
        {
            byte[] tail = new byte[((data.Length - whole) + 127) & ~127];
            data[whole..].CopyTo(tail);
            accum = HashBlocks(tail, parameters, accum);
        }

        UInt128 u = ((UInt128)parameters.PolyU.GetElement(1) << 64) | parameters.PolyU.GetElement(0);
        UInt128 y = ((UInt128)parameters.PolyY.GetElement(1) << 64) | parameters.PolyY.GetElement(0);
        return Multiply(accum ^ u, (uint)data.Length ^ y);
    }

    internal static UInt128 Avalanche(UInt128 raw, PolyXorHashParams parameters, ulong tweak = 0)
    {
        unchecked
        {
            ulong lo = (ulong)raw + tweak, hi = (ulong)(raw >> 64);
            lo ^= lo >> 32;
            lo *= 0x0e9846af9b1a615d;
            hi ^= hi >> 32;
            hi *= 0x0e9846af9b1a615d;
            lo ^= BitOperations.RotateLeft(hi, 32);
            hi += lo;
            lo ^= lo >> 32;
            lo *= 0x0e9846af9b1a615d;
            hi ^= hi >> 32;
            hi *= 0x0e9846af9b1a615d;
            UInt128 mul = ((UInt128)parameters.AvalancheMul.GetElement(1) << 64) | parameters.AvalancheMul.GetElement(0);
            return Multiply(((UInt128)lo << 64) | hi, mul);
        }
    }

    internal static byte[] RandomBytes(int length, ulong seed)
    {
        byte[] bytes = new byte[length];

        for (int i = 0; i < bytes.Length; i++)
        {
            seed = unchecked((seed * 0x5851f42d4c957f2d) + 1);
            bytes[i] = (byte)(seed >> 56);
        }

        return bytes;
    }
}