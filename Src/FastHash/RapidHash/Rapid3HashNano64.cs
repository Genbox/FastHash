using System.Runtime.CompilerServices;
using static Genbox.FastHash.RapidHash.RapidHashConstants;
using static Genbox.FastHash.RapidHash.RapidHashShared;

namespace Genbox.FastHash.RapidHash;

public static class Rapid3HashNano64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input) => ComputeIndex(input, 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed)
    {
        seed ^= RapidMix(seed ^ DefaultSecret2, DefaultSecret1);
        seed ^= 8UL;
        ulong a = input ^ DefaultSecret1;
        ulong b = input ^ seed;

        ulong high = BigMul(a, b, out ulong low);
        return RapidMix(low ^ DefaultSecret7, high ^ DefaultSecret1 ^ 8UL);
    }

    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong[]? secret) => ComputeHash(data, 0, secret);

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed) => ComputeHash(data, seed, null);

    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed, ulong[]? secret)
    {
        secret ??= DefaultSecret;
        ulong secret0 = secret[0];
        ulong secret1 = secret[1];
        ulong secret2 = secret[2];
        ulong secret7 = secret[7];

        seed ^= RapidMix(seed ^ secret2, secret1);

        ulong a = 0;
        ulong b = 0;
        int i = data.Length;
        int offset = 0;

        if (i <= 16)
        {
            if (i >= 4)
            {
                seed ^= (ulong)i;
                if (i >= 8)
                {
                    a = Read64(data);
                    b = Read64(data, i - 8);
                }
                else
                {
                    a = Read32(data);
                    b = Read32(data, i - 4);
                }
            }
            else if (i > 0)
            {
                a = ((ulong)data[0] << 45) | data[i - 1];
                b = data[i >> 1];
            }
        }
        else
        {
            if (i > 48)
            {
                ulong see1 = seed;
                ulong see2 = seed;

                while (i > 48)
                {
                    seed = RapidMix(Read64(data, offset) ^ secret0, Read64(data, offset + 8) ^ seed);
                    see1 = RapidMix(Read64(data, offset + 16) ^ secret1, Read64(data, offset + 24) ^ see1);
                    see2 = RapidMix(Read64(data, offset + 32) ^ secret2, Read64(data, offset + 40) ^ see2);

                    offset += 48;
                    i -= 48;
                }

                seed ^= see1;
                seed ^= see2;
            }

            if (i > 16)
            {
                seed = RapidMix(Read64(data, offset) ^ secret2, Read64(data, offset + 8) ^ seed);
                if (i > 32)
                    seed = RapidMix(Read64(data, offset + 16) ^ secret2, Read64(data, offset + 24) ^ seed);
            }

            a = Read64(data, (offset + i) - 16) ^ (ulong)i;
            b = Read64(data, (offset + i) - 8);
        }

        a ^= secret1;
        b ^= seed;
        RapidMum(ref a, ref b);
        return RapidMix(a ^ secret7, b ^ secret1 ^ (ulong)i);
    }
}