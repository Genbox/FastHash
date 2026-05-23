using System.Runtime.CompilerServices;
using static Genbox.FastHash.WyHash.WyHashConstants;

namespace Genbox.FastHash.WyHash;

public static class Wy4Hash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length) => ComputeHash(data, length, 0);

    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed)
    {
        int i = length;

        if (i > 64)
        {
            ulong see1 = seed;
            ulong see2 = seed;
            ulong see3 = seed;

            do
            {
                seed = Wymum(Read64(data) ^ seed ^ Wyp0, Read64(data + 8) ^ seed ^ Wyp1);
                see1 = Wymum(Read64(data + 16) ^ see1 ^ Wyp2, Read64(data + 24) ^ see1 ^ Wyp3);
                see2 = Wymum(Read64(data + 32) ^ see2 ^ Wyp1, Read64(data + 40) ^ see2 ^ Wyp2);
                see3 = Wymum(Read64(data + 48) ^ see3 ^ Wyp3, Read64(data + 56) ^ see3 ^ Wyp0);

                data += 64;
                i -= 64;
            } while (i >= 64);

            seed ^= see1 ^ see2 ^ see3;
        }

        return ComputeTail(data, i, (ulong)length, seed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong ComputeTail(byte* data, int i, ulong length, ulong seed)
    {
        if (i < 4)
            return Wymum(Wymum((i != 0 ? Wyr3(data, i) : 0) ^ seed ^ Wyp0, seed ^ Wyp1), length ^ Wyp4);

        if (i <= 8)
            return Wymum(Wymum(Read32(data) ^ seed ^ Wyp0, Read32((data + i) - 4) ^ seed ^ Wyp1), length ^ Wyp4);

        if (i <= 16)
            return Wymum(Wymum(Read64(data) ^ seed ^ Wyp0, Read64((data + i) - 8) ^ seed ^ Wyp1), length ^ Wyp4);

        if (i <= 32)
        {
            ulong a = Wymum(Read64(data) ^ seed ^ Wyp0, Read64(data + 8) ^ seed ^ Wyp1);
            ulong b = Wymum(Read64((data + i) - 16) ^ seed ^ Wyp2, Read64((data + i) - 8) ^ seed ^ Wyp3);
            return Wymum(a ^ b, length ^ Wyp4);
        }

        {
            ulong a = Wymum(Read64(data) ^ seed ^ Wyp0, Read64(data + 8) ^ seed ^ Wyp1);
            ulong b = Wymum(Read64(data + 16) ^ seed ^ Wyp2, Read64(data + 24) ^ seed ^ Wyp3);
            ulong c = Wymum(Read64((data + i) - 32) ^ seed ^ Wyp1, Read64((data + i) - 24) ^ seed ^ Wyp2);
            ulong d = Wymum(Read64((data + i) - 16) ^ seed ^ Wyp3, Read64((data + i) - 8) ^ seed ^ Wyp0);
            return Wymum(a ^ b ^ c ^ d, length ^ Wyp4);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong Wyr3(byte* data, int i) => ((ulong)data[0] << 16) | ((ulong)data[i >> 1] << 8) | data[i - 1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Wymum(ulong a, ulong b)
    {
        ulong high = BigMul(a, b, out ulong low);
        return high ^ low;
    }
}