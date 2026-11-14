// C# port of wangyi-fudan/wyhash final version 3 (991aa3d) by Wang Yi.
// Distributed under the Unlicense; see THIRD-PARTY-NOTICES.txt at the repository root.
//#define WYHASH_CONDOM
//WYHASH_CONDOM protections produce different results:
//1: normal valid behavior
//2: extra protection against entropy loss (probability=2^-63), aka. "blind multiplication"
using System.Runtime.CompilerServices;
using static Genbox.FastHash.WyHash.WyHashConstants;
using static Genbox.FastHash.WyHash.WyHashShared;

namespace Genbox.FastHash.WyHash;

/// <summary>Computes 64-bit wyhash final version 3 hashes.</summary>
/// <remarks>Define <c>WYHASH_CONDOM</c> at build time to select upstream mode 2 (blind multiplication); otherwise upstream mode 1 is used.</remarks>
public static class Wy3Hash64
{
    /// <summary>Computes a hash for a 64-bit index.</summary>
    /// <param name="input">The index to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        // Same as hashing the 8 little-endian bytes of input with a zero seed.
        ulong low = (uint)input;
        ulong high = input >> 32;
        ulong a = (low << 32) | high;
        ulong b = (high << 32) | low;
        return Wymix(V3DefaultSecret[1] ^ 8, Wymix(a ^ V3DefaultSecret[1], b ^ V3DefaultSecret[0]));
    }

    /// <summary>Computes a hash for the supplied data using the default secret and a zero seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes a hash for the supplied data using a custom secret and a zero seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="secret">The secret words, or <see langword="null" /> to use the default secret; the first four words are used.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentException"><paramref name="secret" /> contains fewer than four words.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong[]? secret) => ComputeHash(data, 0, secret);

    /// <summary>Computes a hash for the supplied data using the default secret.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed) => ComputeHash(data, seed, null);

    /// <summary>Computes a hash for the supplied data.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <param name="secret">The secret words, or <see langword="null" /> to use the default secret; the first four words are used.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentException"><paramref name="secret" /> contains fewer than four words.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed, ulong[]? secret)
    {
        secret ??= V3DefaultSecret;
        if (secret.Length < 4)
            throw new ArgumentException("The secret must contain at least four words.", nameof(secret));

        int len = data.Length;
        seed ^= secret[0];
        ulong a, b;

        if (len <= 16)
        {
            if (len >= 4)
            {
                int quarter = (len >> 3) << 2;
                a = ((ulong)Read32(data) << 32) | Read32(data, quarter);
                b = ((ulong)Read32(data, len - 4) << 32) | Read32(data, len - 4 - quarter);
            }
            else if (len > 0)
            {
                a = _wyr3(data, len);
                b = 0;
            }
            else
            {
                a = 0;
                b = 0;
            }
        }
        else
        {
            int i = len;
            uint offset = 0;

            if (i > 48)
            {
                ulong see1 = seed, see2 = seed;

                do
                {
                    seed = Wymix(Read64(data, offset) ^ secret[1], Read64(data, offset + 8) ^ seed);
                    see1 = Wymix(Read64(data, offset + 16) ^ secret[2], Read64(data, offset + 24) ^ see1);
                    see2 = Wymix(Read64(data, offset + 32) ^ secret[3], Read64(data, offset + 40) ^ see2);
                    offset += 48;
                    i -= 48;
                } while (i > 48);

                seed ^= see1 ^ see2;
            }

            while (i > 16)
            {
                seed = Wymix(Read64(data, offset) ^ secret[1], Read64(data, offset + 8) ^ seed);
                i -= 16;
                offset += 16;
            }

            a = Read64(data, (uint)((offset + i) - 16));
            b = Read64(data, (uint)((offset + i) - 8));
        }

        return Wymix(secret[1] ^ (uint)len, Wymix(a ^ secret[1], b ^ seed));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong _wyr3(ReadOnlySpan<byte> data, int k) => ((ulong)data[0] << 16) | ((ulong)data[k >> 1] << 8) | data[k - 1];
}