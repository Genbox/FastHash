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

namespace Genbox.FastHash.FarmHash;

public static class FarmHash32Unsafe
{
    private static unsafe uint Hash32Len0to4(byte* s, uint len, uint seed = 0)
    {
        uint b = seed;
        uint c = 9;
        for (int i = 0; i < len; i++)
        {
            b = b * FarmHashConstants.c1 + *(s + i);
            c ^= b;
        }
        return Utilities.FMix(FarmHash.Mur(b, FarmHash.Mur(len, c)));
    }

    private static unsafe uint Hash32Len13to24(byte* s, uint len, uint seed = 0)
    {
        uint a = Utilities.Read32(s - 4 + (len >> 1));
        uint b = Utilities.Read32(s + 4);
        uint c = Utilities.Read32(s + len - 8);
        uint d = Utilities.Read32(s + (len >> 1));
        uint e = Utilities.Read32(s);
        uint f = Utilities.Read32(s + len - 4);
        uint h = d * FarmHashConstants.c1 + len + seed;
        a = Utilities.RotateRightCheck(a, 12) + f;
        h = FarmHash.Mur(c, h) + a;
        a = Utilities.RotateRightCheck(a, 3) + c;
        h = FarmHash.Mur(e, h) + a;
        a = Utilities.RotateRightCheck(a + f, 12) + d;
        h = FarmHash.Mur(b ^ seed, h) + a;
        return Utilities.FMix(h);
    }

    private static unsafe uint Hash32Len5to12(byte* s, uint len, uint seed = 0)
    {
        uint a = len, b = len * 5, c = 9, d = b + seed;
        a += Utilities.Read32(s);
        b += Utilities.Read32(s + len - 4);
        c += Utilities.Read32(s + ((len >> 1) & 4));
        return Utilities.FMix(seed ^ FarmHash.Mur(c, FarmHash.Mur(b, FarmHash.Mur(a, d))));
    }

    public static unsafe uint ComputeHash(byte* s, int len)
    {
        if (len <= 4)
            return Hash32Len0to4(s, (uint)len);

        if (len <= 24)
            return len <= 12 ? Hash32Len5to12(s, (uint)len) : Hash32Len13to24(s, (uint)len);

        uint h = (uint)len, g = FarmHashConstants.c1 * (uint)len, f = g;
        uint a0 = Utilities.RotateRightCheck(Utilities.Read32(s + len - 4) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        uint a1 = Utilities.RotateRightCheck(Utilities.Read32(s + len - 8) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        uint a2 = Utilities.RotateRightCheck(Utilities.Read32(s + len - 16) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        uint a3 = Utilities.RotateRightCheck(Utilities.Read32(s + len - 12) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        uint a4 = Utilities.RotateRightCheck(Utilities.Read32(s + len - 20) * FarmHashConstants.c1, 17) * FarmHashConstants.c2;
        h ^= a0;
        h = Utilities.RotateRightCheck(h, 19);
        h = h * 5 + 0xe6546b64;
        h ^= a2;
        h = Utilities.RotateRightCheck(h, 19);
        h = h * 5 + 0xe6546b64;
        g ^= a1;
        g = Utilities.RotateRightCheck(g, 19);
        g = g * 5 + 0xe6546b64;
        g ^= a3;
        g = Utilities.RotateRightCheck(g, 19);
        g = g * 5 + 0xe6546b64;
        f += a4;
        f = Utilities.RotateRightCheck(f, 19) + 113;
        uint iters = ((uint)len - 1) / 20;
        do
        {
            uint a = Utilities.Read32(s);
            uint b = Utilities.Read32(s + 4);
            uint c = Utilities.Read32(s + 8);
            uint d = Utilities.Read32(s + 12);
            uint e = Utilities.Read32(s + 16);
            h += a;
            g += b;
            f += c;
            h = FarmHash.Mur(d, h) + e;
            g = FarmHash.Mur(c, g) + a;
            f = FarmHash.Mur(b + e * FarmHashConstants.c1, f) + d;
            f += g;
            g += f;
            s += 20;
        } while (--iters != 0);
        g = Utilities.RotateRightCheck(g, 11) * FarmHashConstants.c1;
        g = Utilities.RotateRightCheck(g, 17) * FarmHashConstants.c1;
        f = Utilities.RotateRightCheck(f, 11) * FarmHashConstants.c1;
        f = Utilities.RotateRightCheck(f, 17) * FarmHashConstants.c1;
        h = Utilities.RotateRightCheck(h + g, 19);
        h = h * 5 + 0xe6546b64;
        h = Utilities.RotateRightCheck(h, 17) * FarmHashConstants.c1;
        h = Utilities.RotateRightCheck(h + f, 19);
        h = h * 5 + 0xe6546b64;
        h = Utilities.RotateRightCheck(h, 17) * FarmHashConstants.c1;
        return h;
    }
}