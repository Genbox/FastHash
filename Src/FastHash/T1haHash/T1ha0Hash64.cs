#if NET8_0
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Genbox.FastHash.T1haHash;

public static class T1ha0Hash64
{
    private const ulong prime_0 = 0xEC99BF0D8372CAABUL;
    private const ulong prime_1 = 0x82434FE90EDCEF39UL;
    private const ulong prime_2 = 0xD4F06DB99D67BE4BUL;
    private const ulong prime_3 = 0xBD9CACC22C6E9571UL;
    private const ulong prime_4 = 0x9C06FAF4D023E3ABUL;
    private const ulong prime_5 = 0xC060724A8424F345UL;
    private const ulong prime_6 = 0xCB5AF53AE3AAAC31UL;

    public static unsafe ulong ComputeHash(byte* data, uint len, ulong seed = 0)
    {
        return T1HA_IA32AES_NAME(data, len, seed);
    }

    private static unsafe ulong T1HA_IA32AES_NAME(void* data, uint len, ulong seed)
    {
        ulong a = seed;
        ulong b = len;

        if (len > 32)
        {
            Vector128<byte> x = Vector128.Create(a, b).AsByte(); //TODO: probably swapped
            Vector128<byte> y = Aes.Encrypt(x, Vector128.Create(prime_5, prime_6).AsByte());  //TODO: probably swapped

            byte* v = (byte*)data;
            Vector128<byte>* detent = (Vector128<byte>*)((byte*)data + len - 127);

            while (v < detent)
            {
                Vector128<byte> v0 = Sse2.LoadVector128(v + 0);
                Vector128<byte> v1 = Sse2.LoadVector128(v + 16);
                Vector128<byte> v2 = Sse2.LoadVector128(v + 32);
                Vector128<byte> v3 = Sse2.LoadVector128(v + 48);
                Vector128<byte> v4 = Sse2.LoadVector128(v + 64);
                Vector128<byte> v5 = Sse2.LoadVector128(v + 80);
                Vector128<byte> v6 = Sse2.LoadVector128(v + 96);
                Vector128<byte> v7 = Sse2.LoadVector128(v + 112);

                Vector128<byte> v0y = Aes.Encrypt(v0, y);
                Vector128<byte> v2x6 = Aes.Encrypt(v2, Sse2.Xor(x, v6));
                Vector128<byte> v45_67 = Sse2.Xor(Aes.Encrypt(v4, v5), Sse2.Add(v6, v7));

                Vector128<byte> v0y7_1 = Aes.Decrypt(Sse2.Subtract(v7, v0y), v1);
                Vector128<byte> v2x6_3 = Aes.Encrypt(v2x6, v3);

                x = Aes.Encrypt(v45_67, Sse2.Add(x, y));
                y = Aes.Encrypt(v2x6_3, Sse2.Xor(v0y7_1, v5));
                v += 128;
            }

            if ((len & 64) > 0)
            {
                Vector128<byte> v0y = Sse2.Add(y, Sse2.LoadVector128(v + 0));
                Vector128<byte> v1x = Sse2.Subtract(x, Sse2.LoadVector128(v + 16));
                x = Aes.Decrypt(x, v0y);
                y = Aes.Decrypt(y, v1x);

                Vector128<byte> v2y = Sse2.Add(y, Sse2.LoadVector128(v + 32));
                Vector128<byte> v3x = Sse2.Subtract(x, Sse2.LoadVector128(v + 48));
                x = Aes.Decrypt(x, v2y);
                y = Aes.Decrypt(y, v3x);

                v += 64;
            }

            if ((len & 32) > 0)
            {
                Vector128<byte> v0y = Sse2.Add(y, Sse2.LoadVector128(v + 0));
                Vector128<byte> v1x = Sse2.Subtract(x, Sse2.LoadVector128(v + 16));
                x = Aes.Decrypt(x, v0y);
                y = Aes.Decrypt(y, v1x);
                v += 32;
            }

            if ((len & 16) > 0)
            {
                y = Sse2.Add(x, y);
                x = Aes.Decrypt(x, Sse2.LoadVector128(v + 0));
                v += 16;
            }

            x = Sse2.Add(Aes.Decrypt(x, Aes.Encrypt(y, x)), y);

            var xTmp = x.AsUInt64();
            a = xTmp[0];
            b = xTmp[1];

            data = v;
            len &= 15;
        }

        byte* vPtr = (byte*)data;
        switch (len)
        {
            default:
                mixup64(&a, &b, Read64(vPtr), prime_4);
                vPtr += 8;
                /* fall through */
                goto case 24;
            case 24:
            case 23:
            case 22:
            case 21:
            case 20:
            case 19:
            case 18:
            case 17:
                mixup64(&b, &a, Read64(vPtr), prime_3);
                vPtr += 8;
                /* fall through */
                goto case 16;
            case 16:
            case 15:
            case 14:
            case 13:
            case 12:
            case 11:
            case 10:
            case 9:
                mixup64(&a, &b, Read64(vPtr), prime_2);
                vPtr += 8;
                /* fall through */
                goto case 8;
            case 8:
            case 7:
            case 6:
            case 5:
            case 4:
            case 3:
            case 2:
            case 1:
                mixup64(&b, &a, tail64_le_unaligned(vPtr, len), prime_1);
                /* fall through */
                goto case 0;
            case 0:
                return final64(a, b);
        }
    }

    private static unsafe ulong tail64_le_unaligned(byte* v, uint tail)
    {
        uint offset = (8 - tail) & 7;
        uint shift = offset << 3;
        return Read64(v) & (~0UL >> (int)shift);
    }

    private static ulong final64(ulong a, ulong b)
    {
        ulong x = (a + RotateRight(b, 41)) * prime_0;
        ulong y = (RotateRight(a, 23) + b) * prime_6;
        return mux64(x ^ y, prime_5);
    }

    private static unsafe ulong mux64(ulong v, ulong prime)
    {
        ulong l, h;
        l = mul_64x64_128(v, prime, &h);
        return l ^ h;
    }

    private static unsafe void mixup64(ulong* a, ulong* b, ulong v, ulong prime)
    {
        ulong h;
        *a ^= mul_64x64_128(*b + v, prime, &h);
        *b += h;
    }

    private static unsafe ulong mul_64x64_128(ulong a, ulong b, ulong* h)
    {
        *h = BigMul(a, b, out ulong _);
        return a * b;
    }
}
#endif