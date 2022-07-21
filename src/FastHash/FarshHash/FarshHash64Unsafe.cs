//Copyright(c) 2015-16 Bulat Ziganshin<bulat.ziganshin@gmail.com>
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

//Ported to C# by Ian Qvist
//Website: https://github.com/Bulat-Ziganshin/FARSH
//Source: https://github.com/Bulat-Ziganshin/FARSH/blob/master/farsh.c

namespace Genbox.FastHash.FarshHash;

public static class FarshHash64Unsafe
{
    /* STRIPE bytes of key material plus extra keys for hashes up to 1024 bits long */
    private static unsafe ulong farsh_full_block(uint* data)
    {
        ulong sum = 0;
        int i;

        for (i = 0; i < FarshHashConstants.STRIPE_ELEMENTS; i += 2)
            sum += (data[i] + FarshHashConstants.FARSH_KEYS[i]) * (ulong)(data[i + 1] + FarshHashConstants.FARSH_KEYS[i + 1]);

        return sum;
    }

    private static unsafe ulong farsh_partial_block(uint* data, int length)
    {
        ulong sum = 0;
        int elements = (length / sizeof(uint)) & ~1;
        int i;

        for (i = 0; i < elements; i += 2)
        {
            sum += (data[i] + FarshHashConstants.FARSH_KEYS[i]) * (ulong)(data[i + 1] + FarshHashConstants.FARSH_KEYS[i + 1]);
            length -= 8;
        }

        data += elements;

        uint v1;
        uint v2;

        byte* ptr = (byte*)data;

        switch (length)
        {
            case 7:
                v1 = Utilities.Fetch32(ptr);
                ptr += 4;
                v2 = (uint)(ptr[0] | (ptr[1] << 8) | (ptr[2] << 16));
                sum += (v1 + FarshHashConstants.FARSH_KEYS[i]) * (ulong)(v2 + FarshHashConstants.FARSH_KEYS[i + 1]);
                break;
            case 6:
                v1 = Utilities.Fetch32(ptr);
                ptr += 4;
                v2 = Utilities.Fetch16(ptr);
                sum += (v1 + FarshHashConstants.FARSH_KEYS[i]) * (ulong)(v2 + FarshHashConstants.FARSH_KEYS[i + 1]);
                break;
            case 5:
                v1 = Utilities.Fetch32(ptr);
                ptr += 4;
                v2 = *ptr;
                sum += (v1 + FarshHashConstants.FARSH_KEYS[i]) * (ulong)(v2 + FarshHashConstants.FARSH_KEYS[i + 1]);
                break;
            case 4:
                v1 = Utilities.Fetch32(ptr);
                sum += (v1 + FarshHashConstants.FARSH_KEYS[i]) * (ulong)FarshHashConstants.FARSH_KEYS[i + 1];
                break;
            case 3:
                v1 = (uint)(ptr[0] | (ptr[1] << 8) | (ptr[2] << 16));
                sum += (v1 + FarshHashConstants.FARSH_KEYS[i]) * (ulong)FarshHashConstants.FARSH_KEYS[i + 1];
                break;
            case 2:
                v1 = Utilities.Fetch16(ptr);
                sum += (v1 + FarshHashConstants.FARSH_KEYS[i]) * (ulong)FarshHashConstants.FARSH_KEYS[i + 1];
                break;
            case 1:
                v1 = *ptr;
                sum += (v1 + FarshHashConstants.FARSH_KEYS[i]) * (ulong)FarshHashConstants.FARSH_KEYS[i + 1];
                break;
        }

        return sum;
    }

    public static unsafe ulong ComputeHash(byte* data, int length, ulong seed = 0)
    {
        ulong sum = seed;

        uint* uptr = (uint*)data;

        while (length >= FarshHashConstants.STRIPE)
        {
            ulong h = farsh_full_block(uptr);
            sum = farsh_combine(sum, h);
            uptr += FarshHashConstants.STRIPE_ELEMENTS;
            length -= FarshHashConstants.STRIPE;
        }

        if (length > 0)
        {
            ulong h = farsh_partial_block(uptr, length);
            sum = farsh_combine(sum, h);
        }

        return farsh_final(sum) ^ FarshHashConstants.FARSH_KEYS[0]; /* ensure that zeroes at the end of data will affect the hash value */
    }

    private static ulong farsh_combine(ulong sum, ulong h)
    {
        h *= FarshHashConstants.PRIME64_2;
        h += h >> 31;
        h *= FarshHashConstants.PRIME64_1;
        sum ^= h;
        sum = (sum + (sum >> 27)) * FarshHashConstants.PRIME64_1 + FarshHashConstants.PRIME64_4;
        return sum;
    }

    private static uint farsh_final(ulong sum)
    {
        sum ^= sum >> 33;
        sum *= FarshHashConstants.PRIME64_2;
        sum ^= sum >> 29;
        sum *= FarshHashConstants.PRIME64_3;
        return (uint)sum ^ (uint)(sum >> 32);
    }
}