namespace Genbox.FastHashesNet.wyHash;

public unsafe class wyHash64Unsafe
{
    private static ulong _wyr8(byte* p) => Utilities.Fetch64(p);

    private static ulong _wymix(ulong A, ulong B)
    {
        _wymum(&A, &B);
        return A ^ B;
    }

    private static void _wymum(ulong* A, ulong* B)
    {
        ulong hh = (*A >> 32) * (*B >> 32);
        ulong hl = (*A >> 32) * (uint)*B;
        ulong lh = (uint)*A * (*B >> 32);
        ulong ll = (uint)*A * *B;

        *A = _wyrot(hl) ^ hh;
        *B = _wyrot(lh) ^ ll;
    }

    private static ulong _wyrot(ulong x) => (x >> 32) | (x << 32);

    public static ulong ComputeHash(byte* data, ulong len, ulong seed, ulong* secret)
    {
        byte* p = data;
        ulong a, b;
        seed ^= *secret;

        if (len <= 16)
        {
            int val = len < 8 ? 1 : 0;
            int s = val * ((8 - (int)len) << 3);
            a = _wyr8(p) << s;
            b = _wyr8(p + len - 8) >> s;
        }
        else
        {
            ulong i = len;
            if (i > 48)
            {
                ulong see1 = seed;
                ulong see2 = seed;
                do
                {
                    seed = _wymix(_wyr8(p) ^ secret[1], _wyr8(p + 8) ^ seed);
                    see1 = _wymix(_wyr8(p + 16) ^ secret[2], _wyr8(p + 24) ^ see1);
                    see2 = _wymix(_wyr8(p + 32) ^ secret[3], _wyr8(p + 40) ^ see2);
                    p += 48;
                    i -= 48;
                } while (i > 48);

                seed ^= see1 ^ see2;
            }
            while (i > 16)
            {
                seed = _wymix(_wyr8(p) ^ secret[1], _wyr8(p + 8) ^ seed);
                p += 16;
                i -= 16;
            }
            a = _wyr8(p + i - 16);
            b = _wyr8(p + i - 8);
        }
        return _wymix(secret[1] ^ len, _wymix(a ^ secret[1], b ^ seed));
    }
}