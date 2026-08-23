//#define WYHASH_CONDOM

//WYHASH_CONDOM protections produce different results:
//1: normal valid behavior
//2: extra protection against entropy loss (probability=2^-63), aka. "blind multiplication"

using System.Runtime.CompilerServices;
using static Genbox.FastHash.WyHash.WyHashConstants;

namespace Genbox.FastHash.WyHash;

/// <summary>Computes 64-bit wyhash version 3 hashes.</summary>
public static class Wy3Hash64
{
    /// <summary>Computes a hash for a 64-bit index.</summary>
    /// <param name="input">The index to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        ulong a = ((ulong)(uint)input << 32) | (uint)(input >> 32);
        ulong b = ((ulong)(uint)(input >> 32) << 32) | (uint)input;

        ulong mixed = Fold128To64(a ^ 0xe7037ed1a0b428dbul, b ^ 0xa0761d6478bd642ful);
        return _wymix(0xe7037ed1a0b428dbul ^ 8, mixed);
    }

    /// <summary>Computes a hash for the supplied data using the default secret and a zero seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0);

    /// <summary>Computes a hash for the supplied data using a custom secret and a zero seed.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="secret">The four-word secret, or <see langword="null"/> to use the default secret.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong[]? secret) => ComputeHash(data, 0, secret);

    /// <summary>Computes a hash for the supplied data using the default secret.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed) => ComputeHash(data, seed, null);

    /// <summary>Computes a hash for the supplied data.</summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <param name="secret">The four-word secret, or <see langword="null"/> to use the default secret.</param>
    /// <returns>The 64-bit hash.</returns>
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed, ulong[]? secret)
    {
        secret ??= DefaultSecret;

        int len = data.Length;
        seed ^= secret[0];
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