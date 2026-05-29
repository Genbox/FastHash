#if NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using static Genbox.FastHash.ClHash.ClHashConstants;

namespace Genbox.FastHash.ClHash;

internal static class ClHashShared
{
    internal static ulong[] CreateKey(ulong seed1, ulong seed2)
    {
        ulong[] key = new ulong[Random64BitWordsNeeded];
        CreateKey(seed1, seed2, key);
        return key;
    }

    internal static void CreateKey(ulong seed1, ulong seed2, Span<ulong> key)
    {
        if (key.Length != Random64BitWordsNeeded)
            throw new ArgumentException($"CLHash keys must contain exactly {Random64BitWordsNeeded} 64-bit words.", nameof(key));

        ulong part1 = seed1;
        ulong part2 = seed2;

        for (int i = 0; i < key.Length; i++)
            key[i] = XorShift128Plus(ref part1, ref part2);

        while (key[128] == 0 && key[129] == 1)
        {
            key[128] = XorShift128Plus(ref part1, ref part2);
            key[129] = XorShift128Plus(ref part1, ref part2);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong XorShift128Plus(ref ulong part1, ref ulong part2)
    {
        ulong s1 = part1;
        ulong s0 = part2;
        part1 = s0;
        s1 ^= s1 << 23;
        part2 = s1 ^ s0 ^ (s1 >> 18) ^ (s0 >> 5);
        return part2 + s0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> LeftShift1(Vector128<byte> value) => LeftShift(value, 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> LeftShift2(Vector128<byte> value) => LeftShift(value, 2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> LeftShift(Vector128<byte> value, byte count)
    {
        Vector128<ulong> v = value.AsUInt64();
        Vector128<byte> u64Shift = Sse2.ShiftLeftLogical(v, count).AsByte();
        Vector128<byte> topBits = Sse2.ShiftLeftLogical128BitLane(Sse2.ShiftRightLogical(v, (byte)(64 - count)).AsByte(), sizeof(ulong));
        return Sse2.Or(u64Shift, topBits);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> LazyMod127(Vector128<byte> low, Vector128<byte> high) => Sse2.Xor(Sse2.Xor(low, LeftShift1(high)), LeftShift2(high));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> Mul128By128To128LazyMod127(Vector128<byte> a, Vector128<byte> b)
    {
        Vector128<byte> mix1 = ClMul(a, b, 0x01);
        Vector128<byte> mix2 = ClMul(a, b, 0x10);
        Vector128<byte> low = ClMul(a, b, 0x00);
        Vector128<byte> high = ClMul(a, b, 0x11);
        Vector128<byte> mix = Sse2.Xor(mix1, mix2);
        mix1 = Sse2.ShiftLeftLogical128BitLane(mix, sizeof(ulong));
        mix2 = Sse2.ShiftRightLogical128BitLane(mix, sizeof(ulong));
        low = Sse2.Xor(low, mix1);
        high = Sse2.Xor(high, mix2);
        return LazyMod127(low, high);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> LazyLengthHash(ulong keyLength, ulong length)
    {
        Vector128<byte> lengthVector = Vector128.Create(length, keyLength).AsByte();
        return ClMul(lengthVector, lengthVector, 0x10);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> PrecompReduction64Vector(Vector128<byte> value)
    {
        Vector128<byte> c = Vector128.Create(0x1bUL, 0UL).AsByte();
        Vector128<byte> q2 = ClMul(value, c, 0x01);
        Vector128<byte> table = Vector128.Create(
            0, 27, 54, 45,
            108, 119, 90, 65,
            216, 195, 238, 245,
            180, 175, 130, 153);
        Vector128<byte> q3 = Ssse3.Shuffle(table, Sse2.ShiftRightLogical128BitLane(q2, sizeof(ulong)));
        Vector128<byte> q4 = Sse2.Xor(q2, value);
        return Sse2.Xor(q3, q4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong PrecompReduction64(Vector128<byte> value) => PrecompReduction64Vector(value).AsUInt64().GetElement(0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Simple128To64HashWithLength(Vector128<byte> value, Vector128<byte> key, ulong keyLength, ulong length)
    {
        Vector128<byte> add = Sse2.Xor(value, key);
        Vector128<byte> product = ClMul(add, add, 0x10);
        Vector128<byte> total = Sse2.Xor(product, LazyLengthHash(keyLength, length));
        return PrecompReduction64(total);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> ClMul(Vector128<byte> left, Vector128<byte> right, byte control) => Pclmulqdq.CarrylessMultiply(left.AsInt64(), right.AsInt64(), control).AsByte();
}
#endif