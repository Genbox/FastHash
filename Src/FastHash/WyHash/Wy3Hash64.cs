//#define WYHASH_CONDOM

//WYHASH_CONDOM protections produce different results:
//1: normal valid behavior
//2: extra protection against entropy loss (probability=2^-63), aka. "blind multiplication"

using System.Runtime.CompilerServices;
using static Genbox.FastHash.WyHash.WyHashConstants;

namespace Genbox.FastHash.WyHash;

/// <summary>Computes 64-bit wyhash version 3 hashes.</summary>
/// <remarks>Define <c>WYHASH_CONDOM</c> at build time to select upstream mode 2 (blind multiplication); otherwise upstream mode 1 is used.</remarks>
public static class Wy3Hash64
{
    /// <summary>Computes a hash for a 64-bit index.</summary>
    /// <param name="input">The index to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        ulong seed = V3DefaultSecret[0];
        return _wymix(V3DefaultSecret[1] ^ 8, _wymix((uint)input ^ V3DefaultSecret[1], (uint)(input >> 32) ^ seed));
    }

    /// <summary>Computes a hash for the supplied data using the default secret and a zero seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes a hash for the supplied data using a custom secret and a zero seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="secret">The secret words, or <see langword="null"/> to use the default secret; the first four words are used.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentException"><paramref name="secret"/> contains fewer than four words.</exception>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong[]? secret) => ComputeHash(data, 0, secret);

    /// <summary>Computes a hash for the supplied data using the default secret.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed) => ComputeHash(data, seed, null);

    /// <summary>Computes a hash for the supplied data.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <param name="secret">The secret words, or <see langword="null"/> to use the default secret; the first four words are used.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentException"><paramref name="secret"/> contains fewer than four words.</exception>
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
            if (len <= 8)
            {
                if (len >= 4)
                {
                    a = Read32(data);
                    b = Read32(data, len - 4);
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
                a = Read64(data);
                b = Read64(data, len - 8);
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
                    seed = _wymix(Read64(data, offset) ^ secret[1], Read64(data, offset + 8) ^ seed);
                    see1 = _wymix(Read64(data, offset + 16) ^ secret[2], Read64(data, offset + 24) ^ see1);
                    see2 = _wymix(Read64(data, offset + 32) ^ secret[3], Read64(data, offset + 40) ^ see2);
                    offset += 48;
                    i -= 48;
                } while (i > 48);
                seed ^= see1 ^ see2;
            }
            while (i > 16)
            {
                uint offset1 = offset + 8;
                seed = _wymix(Read64(data, offset) ^ secret[1], Read64(data, offset1) ^ seed);
                i -= 16;
                offset += 16;
            }
            a = Read64(data, (uint)((offset + i) - 16));
            b = Read64(data, (uint)((offset + i) - 8));
        }
        return _wymix(secret[1] ^ (uint)len, _wymix(a ^ secret[1], b ^ seed));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong _wyr3(ReadOnlySpan<byte> data, int offset = 0) => ((ulong)data[0] << 16) | ((ulong)data[offset >> 1] << 8) | data[offset - 1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void _wymum(ref ulong A, ref ulong B)
    {
        ulong high = BigMul(A, B, out ulong low);

#if WYHASH_CONDOM
        A ^= low;
        B ^= high;
#else
        A = low;
        B = high;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong _wymix(ulong A, ulong B)
    {
        _wymum(ref A, ref B);
        return A ^ B;
    }
}