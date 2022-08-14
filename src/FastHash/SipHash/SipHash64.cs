using System.Runtime.CompilerServices;
using static Genbox.FastHash.SipHash.SipHashConstants;

namespace Genbox.FastHash.SipHash;

public static class SipHash64
{
    public static ulong ComputeHash(ReadOnlySpan<byte> data, ulong seed1 = 0, ulong seed2 = 0, byte cRounds = 2, byte dRounds = 4)
    {
        int length = data.Length;
        ulong v0 = v0Init;
        ulong v1 = v1Init;
        ulong v2 = v2Init;
        ulong v3 = v3Init;

        int left = length & 7;
        ulong b = (ulong)length << 56;
        int num = length / 8;
        int offset1 = length - left;
        uint i;

        v3 ^= seed2;
        v2 ^= seed1;
        v1 ^= seed2;
        v0 ^= seed1;

        for (i = 0; i < num; i++)
        {
            ulong m = Read64(data, 8 * i);
            v3 ^= m;

            for (int j = 0; j < cRounds; ++j)
                SipRound(ref v0, ref v1, ref v2, ref v3);

            v0 ^= m;
        }

        switch (left)
        {
            case 7:
                b |= (ulong)data[6 + offset1] << 48;
                goto case 6;
            case 6:
                b |= (ulong)data[5 + offset1] << 40;
                goto case 5;
            case 5:
                b |= (ulong)data[4 + offset1] << 32;
                goto case 4;
            case 4:
                b |= (ulong)data[3 + offset1] << 24;
                goto case 3;
            case 3:
                b |= (ulong)data[2 + offset1] << 16;
                goto case 2;
            case 2:
                b |= (ulong)data[1 + offset1] << 8;
                goto case 1;
            case 1:
                b |= data[0 + offset1];
                break;
            case 0:
                break;
        }

        v3 ^= b;

        for (i = 0; i < cRounds; ++i)
            SipRound(ref v0, ref v1, ref v2, ref v3);

        v0 ^= b;
        v2 ^= 0xFF;

        for (i = 0; i < dRounds; ++i)
            SipRound(ref v0, ref v1, ref v2, ref v3);

        return v0 ^ v1 ^ v2 ^ v3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SipRound(ref ulong v0, ref ulong v1, ref ulong v2, ref ulong v3)
    {
        v0 += v1;
        v1 = RotateLeft(v1, 13);
        v1 ^= v0;
        v0 = RotateLeft(v0, 32);

        v2 += v3;
        v3 = RotateLeft(v3, 16);
        v3 ^= v2;

        v2 += v1;
        v1 = RotateLeft(v1, 17);
        v1 ^= v2;
        v2 = RotateLeft(v2, 32);

        v0 += v3;
        v3 = RotateLeft(v3, 21);
        v3 ^= v0;
    }
}