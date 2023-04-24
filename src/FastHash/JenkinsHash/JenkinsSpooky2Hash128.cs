namespace Genbox.FastHash.JenkinsHash;

public static class JenkinsSpooky2Hash128
{
    // number of ulong's in internal state
    const int sc_numVars = 12;
    const ulong sc_const = 0xdeadbeefdeadbeefUL;

    // size of the internal state
    const int sc_blockSize = sc_numVars * 8;

    // size of buffer of unhashed data, in bytes
    const int sc_bufSize = 2 * sc_blockSize;

    public static UInt128 Short(ReadOnlySpan<byte> data, ulong seed0, ulong seed1)
    {
        int length = data.Length;

        uint remainder = (uint)(length % 32);
        ulong a = seed0;
        ulong b = seed1;
        ulong c = sc_const;
        ulong d = sc_const;

        int p = 0;

        // handle all complete sets of 32 bytes
        if (length > 15)
        {
            while (length > 15)
            {
                a += Read64(data, p + 0);
                b += Read64(data, p + 8);
                c += Read64(data, p + 16);
                d += Read64(data, p + 24);
                ShortMix(ref a, ref b, ref c, ref d);
                length -= 16;
                p += 16;
            }

            //Handle the case of 16+ remaining bytes.
            if (remainder >= 16)
            {
                c += Read64(data, p + 0);
                d += Read64(data, p + 8);
                ShortMix(ref a, ref b, ref c, ref d);
                p += 16;
                remainder -= 16;
            }
        }

        // Handle the last 0..15 bytes, and its length
        d += (ulong)length << 56;
        switch (remainder)
        {
            case 15:
                d += ((ulong)data[p + 14]) << 48;
                goto case 14;
            case 14:
                d += ((ulong)data[p + 13]) << 40;
                goto case 13;
            case 13:
                d += ((ulong)data[p + 12]) << 32;
                goto case 12;
            case 12:
                d += Read32(data, p + 8);
                c += Read64(data, p + 0);
                break;
            case 11:
                d += ((ulong)data[p + 10]) << 16;
                goto case 10;
            case 10:
                d += ((ulong)data[p + 9]) << 8;
                goto case 9;
            case 9:
                d += (ulong)data[p + 8];
                goto case 8;
            case 8:
                c += Read64(data, p);
                break;
            case 7:
                c += ((ulong)data[p + 6]) << 48;
                goto case 6;
            case 6:
                c += ((ulong)data[p + 5]) << 40;
                goto case 5;
            case 5:
                c += ((ulong)data[p + 4]) << 32;
                goto case 4;
            case 4:
                c += Read32(data, p);
                break;
            case 3:
                c += ((ulong)data[p + 2]) << 16;
                goto case 2;
            case 2:
                c += ((ulong)data[p + 1]) << 8;
                goto case 1;
            case 1:
                c += (ulong)data[p + 0];
                break;
            case 0:
                c += sc_const;
                d += sc_const;
                break;
        }

        ShortEnd(ref a, ref b, ref c, ref d);

        return new UInt128(a, b);
    }

    private static void ShortMix(ref ulong h0, ref ulong h1, ref ulong h2, ref ulong h3)
    {
        h2 = RotateLeft(h2,50);  h2 += h3;  h0 ^= h2;
        h3 = RotateLeft(h3,52);  h3 += h0;  h1 ^= h3;
        h0 = RotateLeft(h0,30);  h0 += h1;  h2 ^= h0;
        h1 = RotateLeft(h1,41);  h1 += h2;  h3 ^= h1;
        h2 = RotateLeft(h2,54);  h2 += h3;  h0 ^= h2;
        h3 = RotateLeft(h3,48);  h3 += h0;  h1 ^= h3;
        h0 = RotateLeft(h0,38);  h0 += h1;  h2 ^= h0;
        h1 = RotateLeft(h1,37);  h1 += h2;  h3 ^= h1;
        h2 = RotateLeft(h2,62);  h2 += h3;  h0 ^= h2;
        h3 = RotateLeft(h3,34);  h3 += h0;  h1 ^= h3;
        h0 = RotateLeft(h0,5);   h0 += h1;  h2 ^= h0;
        h1 = RotateLeft(h1,36);  h1 += h2;  h3 ^= h1;
    }

    private static void ShortEnd(ref ulong h0, ref ulong h1, ref ulong h2, ref ulong h3)
    {
        h3 ^= h2;  h2 = RotateLeft(h2,15);  h3 += h2;
        h0 ^= h3;  h3 = RotateLeft(h3,52);  h0 += h3;
        h1 ^= h0;  h0 = RotateLeft(h0,26);  h1 += h0;
        h2 ^= h1;  h1 = RotateLeft(h1,51);  h2 += h1;
        h3 ^= h2;  h2 = RotateLeft(h2,28);  h3 += h2;
        h0 ^= h3;  h3 = RotateLeft(h3,9);   h0 += h3;
        h1 ^= h0;  h0 = RotateLeft(h0,47);  h1 += h0;
        h2 ^= h1;  h1 = RotateLeft(h1,54);  h2 += h1;
        h3 ^= h2;  h2 = RotateLeft(h2,32);  h3 += h2;
        h0 ^= h3;  h3 = RotateLeft(h3,25);  h0 += h3;
        h1 ^= h0;  h0 = RotateLeft(h0,63);  h1 += h0;
    }

    public static UInt128 ComputeHash(ReadOnlySpan<byte> data, ulong seed0, ulong seed1)
    {
        int length = data.Length;

        if (length < sc_bufSize)
            return Short(data, seed0, seed1);

        ulong h0, h1, h2, h3, h4, h5, h6, h7, h8, h9, h10, h11;
        int remainder;

        h0 = h3 = h6 = h9 = seed0;
        h1 = h4 = h7 = h10 = seed1;
        h2 = h5 = h8 = h11 = sc_const;

        // handle all whole sc_blockSize blocks of bytes
        while (length > 15)
        {
            Mix(data, h0, h1, h2, h3, h4, h5, h6, h7, h8, h9, h10, h11);
            u.p64 += sc_numVars;
            length -= 16;
        }

        // handle the last partial block of sc_blockSize bytes
        remainder = (length - ((const uint8*  )end - (const uint8*  )data));
        memcpy(buf, end, remainder);
        memset(((uint8*)buf) + remainder, 0, sc_blockSize - remainder);
        ((uint8*)buf)[sc_blockSize - 1] = remainder;

        // do some final mixing
        End(data, h0, h1, h2, h3, h4, h5, h6, h7, h8, h9, h10, h11);

        return new UInt128(h0, h1);
    }

    private static  void Mix(ulong[] data, ref ulong s0, ref ulong s1, ref ulong s2, ref ulong s3,ref ulong s4, ref ulong s5, ref ulong s6, ref ulong s7, ref ulong s8, ref ulong s9, ref ulong s10, ref ulong s11)
    {
        s0 += data[0]; s2 ^= s10; s11 ^= s0; s0 = RotateLeft(s0,11); s11 += s1;
        s1 += data[1]; s3 ^= s11; s0 ^= s1; s1 = RotateLeft(s1,32); s0 += s2;
        s2 += data[2]; s4 ^= s0; s1 ^= s2; s2 = RotateLeft(s2,43); s1 += s3;
        s3 += data[3]; s5 ^= s1; s2 ^= s3; s3 = RotateLeft(s3,31); s2 += s4;
        s4 += data[4]; s6 ^= s2; s3 ^= s4; s4 = RotateLeft(s4,17); s3 += s5;
        s5 += data[5]; s7 ^= s3; s4 ^= s5; s5 = RotateLeft(s5,28); s4 += s6;
        s6 += data[6]; s8 ^= s4; s5 ^= s6; s6 = RotateLeft(s6,39); s5 += s7;
        s7 += data[7]; s9 ^= s5; s6 ^= s7; s7 = RotateLeft(s7,57); s6 += s8;
        s8 += data[8]; s10 ^= s6; s7 ^= s8; s8 = RotateLeft(s8,55); s7 += s9;
        s9 += data[9]; s11 ^= s7; s8 ^= s9; s9 = RotateLeft(s9,54); s8 += s10;
        s10 += data[10]; s0 ^= s8; s9 ^= s10; s10 = RotateLeft(s10,22); s9 += s11;
        s11 += data[11]; s1 ^= s9; s10 ^= s11; s11 = RotateLeft(s11,46); s10 += s0;
    }
}