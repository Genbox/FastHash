using System.Runtime.CompilerServices;

namespace Genbox.FastHash.JenkinsHash;

public static class JenkinsLookup2Hash96
{
    public static UInt96 ComputeHash(ReadOnlySpan<byte> data, uint seed = 0)
    {
        uint len = (uint)data.Length;
        uint a = 0x9e3779b9;
        uint b = 0x9e3779b9;
        uint c = seed;

        /*---------------------------------------- handle most of the key */
        int p = 0;
        while (len >= 12)
        {
            a += Read32(data, p + 0);
            b += Read32(data, p + 4);
            c += Read32(data, p + 8);

            Mix(ref a, ref b, ref c);

            len -= 12;
            p += 12;
        }

        /*------------------------------------- handle the last 11 bytes */
        c += (uint)data.Length;
        switch (len) /* all the case statements fall through */
        {
            case 11:
                c += (uint)data[p + 10] << 24;
                goto case 10;
            case 10:
                c += (uint)data[p + 9] << 16;
                goto case 9;
            case 9:
                c += (uint)data[p + 8] << 8;
                goto case 8;
            /* the first byte of c is reserved for the length */
            case 8:
                b += (uint)data[p + 7] << 24;
                goto case 7;
            case 7:
                b += (uint)data[p + 6] << 16;
                goto case 6;
            case 6:
                b += (uint)data[p + 5] << 8;
                goto case 5;
            case 5:
                b += data[p + 4];
                goto case 4;
            case 4:
                a += (uint)data[p + 3] << 24;
                goto case 3;
            case 3:
                a += (uint)data[p + 2] << 16;
                goto case 2;
            case 2:
                a += (uint)data[p + 1] << 8;
                goto case 1;
            case 1:
                a += data[p + 0];
                break;
            /* case 0: nothing left to add */
        }

        Mix(ref a, ref b, ref c);

        return new UInt96(a, b, c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Mix(ref uint a, ref uint b, ref uint c)
    {
        a -= b;
        a -= c;
        a ^= c >> 13;
        b -= c;
        b -= a;
        b ^= a << 8;
        c -= a;
        c -= b;
        c ^= b >> 13;
        a -= b;
        a -= c;
        a ^= c >> 12;
        b -= c;
        b -= a;
        b ^= a << 16;
        c -= a;
        c -= b;
        c ^= b >> 5;
        a -= b;
        a -= c;
        a ^= c >> 3;
        b -= c;
        b -= a;
        b ^= a << 10;
        c -= a;
        c -= b;
        c ^= b >> 15;
    }
}