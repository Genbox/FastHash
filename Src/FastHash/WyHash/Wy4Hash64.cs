// C# port of wangyi-fudan/wyhash 4.3.0 (2ac9a50) by Wang Yi.
// Distributed under the Unlicense; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using static Genbox.FastHash.WyHash.WyHashConstants;

namespace Genbox.FastHash.WyHash;

/// <summary>Computes 64-bit wyhash final version 4.3 hashes.</summary>
/// <remarks>Define <c>WYHASH_CONDOM</c> at build time to select upstream mode 2 (blind multiplication); otherwise upstream mode 1 is used.</remarks>
public static class Wy4Hash64
{
    /// <summary>Computes a hash for a 64-bit index using the default secret and a zero seed.</summary>
    /// <param name="input">The index to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input) => ComputeIndex(input, 0);

    /// <summary>Computes a hash for a 64-bit index using the default secret.</summary>
    /// <param name="input">The index to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed)
    {
        ulong[] secret = V4DefaultSecret;
        seed ^= Wymix(seed ^ secret[0], secret[1]);

        ulong a = ((ulong)(uint)input << 32) | (uint)(input >> 32);
        ulong b = ((ulong)(uint)(input >> 32) << 32) | (uint)input;
        a ^= secret[1];
        b ^= seed;
        Wymum(ref a, ref b);
        return Wymix(a ^ secret[0] ^ 8, b ^ secret[1]);
    }

    /// <summary>Computes a hash for the supplied data using the default secret and a zero seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0, null);

    /// <summary>Computes a hash for the supplied data using a custom secret and a zero seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="secret">The secret containing at least four words, or <see langword="null" /> to use the default secret.</param>
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
    /// <param name="secret">The secret containing at least four words, or <see langword="null" /> to use the default secret.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentException"><paramref name="secret" /> contains fewer than four words.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed, ulong[]? secret)
    {
        secret ??= V4DefaultSecret;
        if (secret.Length < 4)
            throw new ArgumentException("The secret must contain at least four words.", nameof(secret));

        seed ^= Wymix(seed ^ secret[0], secret[1]);

        int len = data.Length;
        ulong a, b;

        if (len <= 16)
        {
            if (len >= 4)
            {
                a = ((ulong)Read32(data) << 32) | Read32(data, (len >> 3) << 2);
                b = ((ulong)Read32(data, len - 4) << 32) | Read32(data, len - 4 - ((len >> 3) << 2));
            }
            else if (len > 0)
            {
                a = Wyr3(data, len);
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
            int offset = 0;

            if (i >= 48)
            {
                ulong see1 = seed;
                ulong see2 = seed;

                do
                {
                    seed = Wymix(Read64(data, offset) ^ secret[1], Read64(data, offset + 8) ^ seed);
                    see1 = Wymix(Read64(data, offset + 16) ^ secret[2], Read64(data, offset + 24) ^ see1);
                    see2 = Wymix(Read64(data, offset + 32) ^ secret[3], Read64(data, offset + 40) ^ see2);
                    offset += 48;
                    i -= 48;
                } while (i >= 48);

                seed ^= see1 ^ see2;
            }

            while (i > 16)
            {
                seed = Wymix(Read64(data, offset) ^ secret[1], Read64(data, offset + 8) ^ seed);
                offset += 16;
                i -= 16;
            }

            a = Read64(data, (offset + i) - 16);
            b = Read64(data, (offset + i) - 8);
        }

        a ^= secret[1];
        b ^= seed;
        Wymum(ref a, ref b);
        return Wymix(a ^ secret[0] ^ (uint)len, b ^ secret[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Wyr3(ReadOnlySpan<byte> data, int len) => ((ulong)data[0] << 16) | ((ulong)data[len >> 1] << 8) | data[len - 1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Wymum(ref ulong a, ref ulong b)
    {
        ulong high = BigMul(a, b, out ulong low);
#if WYHASH_CONDOM
        a ^= low;
        b ^= high;
#else
        a = low;
        b = high;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Wymix(ulong a, ulong b)
    {
        Wymum(ref a, ref b);
        return a ^ b;
    }
}