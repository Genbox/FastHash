//#define WYHASH_CONDOM

//WYHASH_CONDOM protections produce different results:
//1: normal valid behavior
//2: extra protection against entropy loss (probability=2^-63), aka. "blind multiplication"

using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace Genbox.FastHash.WyHash;

public class Wy3Hash64
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ComputeIndex(ulong input, ulong seed = 0)
    {
        ulong a = (ulong)(uint)input << 32 | (uint)(input >> 32);
        ulong b = (ulong)(uint)(input >> 32) << 32 | (uint)input;

        seed ^= 0xa0761d6478bd642ful;
        return _wymix(0xe7037ed1a0b428dbul ^ 8, _wymix(a ^ 0xe7037ed1a0b428dbul, b ^ seed));
    }

    public static ulong ComputeHash(byte[] data, ulong seed = 0, ulong[]? secret = null)
    {
        secret ??= WyHashConstants.DefaultSecret;

        uint len = (uint)data.Length;
        seed ^= secret[0];
        ulong a, b;

        if (len <= 16)
        {
            if (len >= 4)
            {
                uint offset = (len >> 3) << 2;
                a = ((ulong)Utilities.Read32(data) << 32) | Utilities.Read32(data, offset);
                uint offset1 = len - 4;
                uint offset2 = len - 4 - ((len >> 3) << 2);
                b = ((ulong)Utilities.Read32(data, offset1) << 32) | Utilities.Read32(data, offset2);
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
                    uint offset1 = offset + 8;
                    seed = _wymix(Utilities.Read64(data, offset) ^ secret[1], Utilities.Read64(data, offset1) ^ seed);
                    uint offset2 = offset + 16;
                    uint offset3 = offset + 24;
                    see1 = _wymix(Utilities.Read64(data, offset2) ^ secret[2], Utilities.Read64(data, offset3) ^ see1);
                    uint offset4 = offset + 32;
                    uint offset5 = offset + 40;
                    see2 = _wymix(Utilities.Read64(data, offset4) ^ secret[3], Utilities.Read64(data, offset5) ^ see2);
                    offset += 48;
                    i -= 48;
                } while (i > 48);
                seed ^= see1 ^ see2;
            }
            while (i > 16)
            {
                uint offset1 = offset + 8;
                seed = _wymix(Utilities.Read64(data, offset) ^ secret[1], Utilities.Read64(data, offset1) ^ seed);
                i -= 16;
                offset += 16;
            }
            uint offset6 = offset + i - 16;
            a = Utilities.Read64(data, offset6);
            uint offset7 = offset + i - 8;
            b = Utilities.Read64(data, offset7);
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