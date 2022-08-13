//#define WYHASH_CONDOM

//WYHASH_CONDOM protections produce different results:
//1: normal valid behavior
//2: extra protection against entropy loss (probability=2^-63), aka. "blind multiplication"

using System.Runtime.CompilerServices;
using static Genbox.FastHash.WyHash.WyHashConstants;

namespace Genbox.FastHash.WyHash;

public class Wy3Hash64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input)
    {
        ulong a = ((ulong)(uint)input << 32) | (uint)(input >> 32);
        ulong b = ((ulong)(uint)(input >> 32) << 32) | (uint)input;

        ulong high = Math.BigMul(a ^ 0xe7037ed1a0b428dbul, b ^ 0xa0761d6478bd642ful, out ulong low);
        return _wymix(0xe7037ed1a0b428dbul ^ 8, low ^ high);
    }

    public static ulong ComputeHash(byte[] data, ulong seed = 0, ulong[]? secret = null)
    {
        secret ??= DefaultSecret;

        uint len = (uint)data.Length;
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
            uint i = len;
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
            a = Read64(data, offset + i - 16);
            b = Read64(data, offset + i - 8);
        }
        return _wymix(secret[1] ^ len, _wymix(a ^ secret[1], b ^ seed));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong _wyr3(byte[] data, uint offset = 0)
    {
        return ((ulong)data[0] << 16) | ((ulong)data[offset >> 1] << 8) | data[offset - 1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void _wymum(ref ulong A, ref ulong B)
    {
        ulong high = Math.BigMul(A, B, out ulong low);

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