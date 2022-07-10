// Copyright (c) 2014 Google, Inc.
//
// FarmHash, by Geoff Pike
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

//Ported to C# by Ian Qvist
//Source: https://github.com/google/farmhash

namespace FastHashesNet.FarmHash;

public static class FarmHash32
{
    private static uint Hash32Len0to4(byte[] s, uint len, uint seed = 0)
    {
        uint b = seed;
        uint c = 9;
        for (int i = 0; i < len; i++)
        {
            byte v = s[i];
            b = b * FarmHashConstants.c1 + v;
            c ^= b;
        }
        return Utilities.FMix(FarmHash.Mur(b, FarmHash.Mur(len, c)));
    }

    private static uint Hash32Len13to24(byte[] s, uint len, uint seed = 0)
    {
        uint a = Utilities.Fetch32(s, (len >> 1) - 4);
        uint b = Utilities.Fetch32(s, 4);
        uint c = Utilities.Fetch32(s, len - 8);
        uint d = Utilities.Fetch32(s, len >> 1);
        uint e = Utilities.Fetch32(s, 0);
        uint f = Utilities.Fetch32(s, len - 4);
        uint h = d * FarmHashConstants.c1 + len + seed;
        a = Utilities.RotateWithCheck(a, 12) + f;
        h = FarmHash.Mur(c, h) + a;
        a = Utilities.RotateWithCheck(a, 3) + c;
        h = FarmHash.Mur(e, h) + a;
        a = Utilities.RotateWithCheck(a + f, 12) + d;
        h = FarmHash.Mur(b ^ seed, h) + a;
        return Utilities.FMix(h);
    }

    private static uint Hash32Len5to12(byte[] s, uint len, uint seed = 0)
    {
        uint a = len, b = len * 5, c = 9, d = b + seed;
        a += Utilities.Fetch32(s, 0u);
        b += Utilities.Fetch32(s, len - 4);
        c += Utilities.Fetch32(s, (len >> 1) & 4);
        return Utilities.FMix(seed ^ FarmHash.Mur(c, FarmHash.Mur(b, FarmHash.Mur(a, d))));
    }

    public static uint ComputeHash(byte[] s)
    {
        uint len = (uint)s.Length;

        if (len <= 24)
            return len <= 12 ? (len <= 4 ? Hash32Len0to4(s, len) : Hash32Len5to12(s, len)) : Hash32Len13to24(s, len);

        // len > 24
        uint h = len, g = FarmHashConstants.c1 * len, f = g;
        uint a0 = Utilities.RotateWithCheck(Utilities.Fetch32(s, len - 4) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        uint a1 = Utilities.RotateWithCheck(Utilities.Fetch32(s, len - 8) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        uint a2 = Utilities.RotateWithCheck(Utilities.Fetch32(s, len - 16) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        uint a3 = Utilities.RotateWithCheck(Utilities.Fetch32(s, len - 12) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        uint a4 = Utilities.RotateWithCheck(Utilities.Fetch32(s, len - 20) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        h ^= a0;
        h = Utilities.RotateWithCheck(h, 19);
        h = h * 5 + 0xe6546b64;
        h ^= a2;
        h = Utilities.RotateWithCheck(h, 19);
        h = h * 5 + 0xe6546b64;
        g ^= a1;
        g = Utilities.RotateWithCheck(g, 19);
        g = g * 5 + 0xe6546b64;
        g ^= a3;
        g = Utilities.RotateWithCheck(g, 19);
        g = g * 5 + 0xe6546b64;
        f += a4;
        f = Utilities.RotateWithCheck(f, 19) + 113;
        uint iters = (len - 1) / 20;
        int index = 0;
        do
        {
            uint a = Utilities.Fetch32(s, index);
            uint b = Utilities.Fetch32(s, index + 4);
            uint c = Utilities.Fetch32(s, index + 8);
            uint d = Utilities.Fetch32(s, index + 12);
            uint e = Utilities.Fetch32(s, index + 16);
            h += a;
            g += b;
            f += c;
            h = FarmHash.Mur(d, h) + e;
            g = FarmHash.Mur(c, g) + a;
            f = FarmHash.Mur(b + e * FarmHashConstants.c1, f) + d;
            f += g;
            g += f;
            index += 20;
        } while (--iters != 0);
        g = Utilities.RotateWithCheck(g, 11) * FarmHashConstants.c1;
        g = Utilities.RotateWithCheck(g, 17) * FarmHashConstants.c1;
        f = Utilities.RotateWithCheck(f, 11) * FarmHashConstants.c1;
        f = Utilities.RotateWithCheck(f, 17) * FarmHashConstants.c1;
        h = Utilities.RotateWithCheck(h + g, 19);
        h = h * 5 + 0xe6546b64;
        h = Utilities.RotateWithCheck(h, 17) * FarmHashConstants.c1;
        h = Utilities.RotateWithCheck(h + f, 19);
        h = h * 5 + 0xe6546b64;
        h = Utilities.RotateWithCheck(h, 17) * FarmHashConstants.c1;
        return h;
    }
}