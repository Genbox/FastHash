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

/// <summary>Computes 64-bit wyhash final version 3 hashes from unmanaged memory.</summary>
/// <remarks>Define <c>WYHASH_CONDOM</c> at build time to select upstream mode 2 (blind multiplication); otherwise upstream mode 1 is used.</remarks>
public static class Wy3Hash64Unsafe
{
    /// <summary>Computes a hash using the default secret and a zero seed.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> readable bytes; it may be null only when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        return ComputeHash(data, length, 0);
    }

    /// <summary>Computes a hash using the default secret.</summary>
    /// <param name="data">A pointer to at least <paramref name="length" /> readable bytes; it may be null only when <paramref name="length" /> is zero.</param>
    /// <param name="length">The number of bytes to hash.</param>
    /// <param name="seed">The hash seed.</param>
    /// <returns>The 64-bit hash.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length" /> is negative.</exception>
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        fixed (ulong* secret = V3DefaultSecret)
        {
            uint len = (uint)length;
            seed ^= secret[0];
            ulong a, b;

            if (len <= 16)
            {
                if (len >= 4)
                {
                    uint quarter = (len >> 3) << 2;
                    a = ((ulong)Read32(data) << 32) | Read32(data + quarter);
                    b = ((ulong)Read32((data + len) - 4) << 32) | Read32((data + len) - 4 - quarter);
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
                uint i = len;

                if (i > 48)
                {
                    ulong see1 = seed, see2 = seed;

                    do
                    {
                        seed = Wymix(Read64(data) ^ secret[1], Read64(data + 8) ^ seed);
                        see1 = Wymix(Read64(data + 16) ^ secret[2], Read64(data + 24) ^ see1);
                        see2 = Wymix(Read64(data + 32) ^ secret[3], Read64(data + 40) ^ see2);
                        data += 48;
                        i -= 48;
                    } while (i > 48);

                    seed ^= see1 ^ see2;
                }

                while (i > 16)
                {
                    seed = Wymix(Read64(data) ^ secret[1], Read64(data + 8) ^ seed);
                    i -= 16;
                    data += 16;
                }

                a = Read64((data + i) - 16);
                b = Read64((data + i) - 8);
            }

            return Wymix(secret[1] ^ len, Wymix(a ^ secret[1], b ^ seed));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong _wyr3(byte* data, uint k) => ((ulong)data[0] << 16) | ((ulong)data[k >> 1] << 8) | data[k - 1];

}