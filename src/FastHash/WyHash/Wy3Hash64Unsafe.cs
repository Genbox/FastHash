//#define WYHASH_CONDOM

//WYHASH_CONDOM protections produce different results:
//1: normal valid behavior
//2: extra protection against entropy loss (probability=2^-63), aka. "blind multiplication"

using System.Runtime.CompilerServices;

namespace Genbox.FastHash.WyHash;

public class Wy3Hash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed = 0)
    {
        fixed (ulong* secret = WyHashConstants.DefaultSecret)
        {
            uint len = (uint)length;
            seed ^= secret[0];
            ulong a, b;

            if (len <= 16)
            {
                if (len >= 4)
                {
                    a = ((ulong)Utilities.Read32(data) << 32) | Utilities.Read32(data + ((len >> 3) << 2));
                    b = ((ulong)Utilities.Read32(data + (len - 4)) << 32) | Utilities.Read32(data + (len - 4 - ((len >> 3) << 2)));
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
                        seed = _wymix(Utilities.Read64(data) ^ secret[1], Utilities.Read64(data + 8) ^ seed);
                        see1 = _wymix(Utilities.Read64(data + 16) ^ secret[2], Utilities.Read64(data + 24) ^ see1);
                        see2 = _wymix(Utilities.Read64(data + 32) ^ secret[3], Utilities.Read64(data + 40) ^ see2);
                        data += 48;
                        i -= 48;
                    } while (i > 48);
                    seed ^= see1 ^ see2;
                }
                while (i > 16)
                {
                    seed = _wymix(Utilities.Read64(data) ^ secret[1], Utilities.Read64(data + 8) ^ seed);
                    i -= 16;
                    data += 16;
                }
                a = Utilities.Read64(data + i - 16);
                b = Utilities.Read64(data + i - 8);
            }
            return _wymix(secret[1] ^ len, _wymix(a ^ secret[1], b ^ seed));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong _wyr3(byte* data, uint offset = 0)
    {
        return ((ulong)data[0] << 16) | ((ulong)data[offset >> 1] << 8) | data[offset - 1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void _wymum(ulong* A, ulong* B)
    {
        ulong low;
        ulong high = Math.BigMul(*A, *B, out low);

#if WYHASH_CONDOM
        *A ^= low;
        *B ^= high;
#else
        *A = low;
        *B = high;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe ulong _wymix(ulong A, ulong B)
    {
        _wymum(&A, &B);
        return A ^ B;
    }
}