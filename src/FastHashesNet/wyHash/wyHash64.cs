//#define WYHASH_CONDOM

namespace FastHashesNet.wyHash;

public class wyHash64
{
    static ulong _wyr8(ulong p, byte[] data)
    {
        unchecked
        {
            return Utilities.Fetch64(data, (int)p);
        }
    }

    static ulong _wymix(ulong A, ulong B)
    {
        ulong hh = (A >> 32) * (B >> 32);
        ulong hl = (A >> 32) * (uint)B;
        ulong lh = (uint)A * (B >> 32);
        ulong ll = (ulong)(uint)A * (uint)B;

#if WYHASH_CONDOM
            A ^= _wyrot(hl) ^ hh;
            B ^= _wyrot(lh) ^ ll;
#else
        A = _wyrot(hl) ^ hh;
        B = _wyrot(lh) ^ ll;
#endif
        return A ^ B;
    }

    static ulong _wyrot(ulong x) { return (x >> 32) | (x << 32); }

    public static ulong ComputeHash(byte[] data, ulong seed, ulong[] secret)
    {
        uint len = (uint)data.Length;

        ulong p = 0;
        ulong a;
        ulong b;
        seed ^= secret[0];

        if (len <= 16)
        {
#if WYHASH_CONDOM
                if (len <= 8)
                {
                    if (len >= 4)
                    {
                        a = _wyr4(p);
                        b = _wyr4(p + len - 4);
                    }
                    else if (len > 0)
                    {
                        a = _wyr3(p, len);
                        b = 0;
                    }
                    else
                        a = b = 0;
                }
                else
                {
                    a = _wyr8(p, data);
                    b = _wyr8(p + len - 8, data);
                }
#else
            int val2 = len < 8 ? 1 : 0;

            int s = val2 * ((8 - (int)len) << 3);
            a = _wyr8(p, data) << s;
            b = _wyr8(p + len - 8, data) >> s;
#endif
        }
        else
        {
            uint i = len;
            if (i > 48)
            {
                ulong see1 = seed;
                ulong see2 = seed;

                do
                {
                    seed = _wymix(_wyr8(p, data) ^ secret[1], _wyr8(p + 8, data) ^ seed);
                    see1 = _wymix(_wyr8(p + 16, data) ^ secret[2], _wyr8(p + 24, data) ^ see1);
                    see2 = _wymix(_wyr8(p + 32, data) ^ secret[3], _wyr8(p + 40, data) ^ see2);
                    i -= 48;
                    p += 48;
                } while (i > 48);
                seed ^= see1 ^ see2;
            }

            while (i > 16)
            {
                seed = _wymix(_wyr8(p, data) ^ secret[1], _wyr8(p + 8, data) ^ seed);
                i -= 16;
                p += 16;
            }

            a = _wyr8(p + i - 16, data);
            b = _wyr8(p + i - 8, data);
        }

        return _wymix((secret[1] ^ len), _wymix(a ^ secret[1], b ^ seed));
    }
}