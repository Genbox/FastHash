using System.Runtime.CompilerServices;
using static Genbox.FastHash.RapidHash.RapidHashConstants;
using static Genbox.FastHash.RapidHash.RapidHashShared;

namespace Genbox.FastHash.RapidHash;

/// <summary>Provides the smallest 64-bit RapidHash v3 variant.</summary>
public static class Rapid3HashNano64
{
    /// <summary>Computes a hash index for <paramref name="input" />.</summary>
    /// <param name="input">The value to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash index.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed = 0) => RapidHashShared.ComputeIndex(input, seed);

    /// <summary>Computes a 64-bit hash for <paramref name="data" />.</summary>
    /// <param name="data">The bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <param name="secret">An optional array containing at least eight secret values.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed = 0, ulong[]? secret = null)
    {
        secret ??= DefaultSecret;
        ulong secret0 = secret[0];
        ulong secret1 = secret[1];
        ulong secret2 = secret[2];
        ulong secret7 = secret[7];

        seed = InitializeSeed(seed, secret1, secret2);

        int i = data.Length;
        int offset = 0;

        if (!TryReadSmall(data, ref seed, out ulong a, out ulong b))
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

            MixRemainingBlocks(data, offset, i, ref seed, secret1, secret2, 2);
            ReadLongTail(data, offset, i, out a, out b);
        }

        return FinalizeHash(i, a, b, seed, secret1, secret7);
    }
}