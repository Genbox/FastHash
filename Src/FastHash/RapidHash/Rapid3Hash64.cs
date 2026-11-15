// C# port of Nicoshev/rapidhash 3.0.0 (bc4b4ba). Copyright 2025 Nicolas De Carli.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Genbox.FastHash.RapidHash.RapidHashConstants;
using static Genbox.FastHash.RapidHash.RapidHashShared;

namespace Genbox.FastHash.RapidHash;

/// <summary>Provides the 64-bit RapidHash v3 algorithm.</summary>
public static class Rapid3Hash64
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
        ref byte dataReference = ref MemoryMarshal.GetReference(data);
        secret ??= DefaultSecret;
        ulong secret0 = secret[0];
        ulong secret1 = secret[1];
        ulong secret2 = secret[2];
        ulong secret3 = secret[3];
        ulong secret4 = secret[4];
        ulong secret5 = secret[5];
        ulong secret6 = secret[6];
        ulong secret7 = secret[7];

        seed = InitializeSeed(seed, secret1, secret2);

        int i = data.Length;
        int offset = 0;

        if (!TryReadSmall(data, ref seed, out ulong a, out ulong b))
        {
            if (i > 112)
            {
                ulong see1 = seed;
                ulong see2 = seed;
                ulong see3 = seed;
                ulong see4 = seed;
                ulong see5 = seed;
                ulong see6 = seed;

                ref byte current = ref dataReference;
                while (i > 112)
                {
                    seed = RapidMix(Read64(ref current) ^ secret0, Read64(ref current, 8) ^ seed);
                    see1 = RapidMix(Read64(ref current, 16) ^ secret1, Read64(ref current, 24) ^ see1);
                    see2 = RapidMix(Read64(ref current, 32) ^ secret2, Read64(ref current, 40) ^ see2);
                    see3 = RapidMix(Read64(ref current, 48) ^ secret3, Read64(ref current, 56) ^ see3);
                    see4 = RapidMix(Read64(ref current, 64) ^ secret4, Read64(ref current, 72) ^ see4);
                    see5 = RapidMix(Read64(ref current, 80) ^ secret5, Read64(ref current, 88) ^ see5);
                    see6 = RapidMix(Read64(ref current, 96) ^ secret6, Read64(ref current, 104) ^ see6);

                    current = ref Unsafe.Add(ref current, 112);
                    i -= 112;
                }

                offset = data.Length - i;

                seed ^= see1;
                see2 ^= see3;
                see4 ^= see5;
                seed ^= see6;
                see2 ^= see4;
                seed ^= see2;
            }

            MixRemainingBlocks(data, offset, i, ref seed, secret1, secret2, 6);
            ReadLongTail(data, offset, i, out a, out b);
        }

        return FinalizeHash(i, a, b, seed, secret1, secret7);
    }
}