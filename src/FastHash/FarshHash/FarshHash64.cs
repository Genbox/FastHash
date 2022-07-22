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

public static class FarshHash64
{
    /* STRIPE bytes of key material plus extra keys for hashes up to 1024 bits long */
    private static ulong farsh_full_block(byte[] data, uint offset)
    {
        ulong sum = 0;
        uint i;
        uint j = 0;
        for (i = 0; i < FarshHashConstants.STRIPE; i += 8, j += 2)
        {
            uint val1 = Utilities.Read32(data, offset + i);
            uint val2 = Utilities.Read32(data, offset + i + sizeof(uint));
            sum += (val1 + FarshHashConstants.FARSH_KEYS[j]) * (ulong)(val2 + FarshHashConstants.FARSH_KEYS[j + 1]);
        }

        return sum;
    }

    private static ulong farsh_partial_block(byte[] data, uint offset)
    {
        ulong sum = 0;
        int keyindex = 0;
        uint chunks = (uint)((data.Length - offset) >> 3);

        for (; chunks > 0; chunks--)
        {
            uint val1 = Utilities.Read32(data, offset);
            uint val2 = Utilities.Read32(data, offset + sizeof(uint));
            sum += (val1 + FarshHashConstants.FARSH_KEYS[keyindex]) * (ulong)(val2 + FarshHashConstants.FARSH_KEYS[keyindex + 1]);
            offset += 8;
            keyindex += 2;
        }

        uint v1;
        uint v2;

        uint remaining = (uint)(data.Length - offset);

        switch (remaining)
        {
            case 7:
                v1 = Utilities.Read32(data, offset);
                offset += 4;
                v2 = (uint)(data[0 + offset] | (data[1 + offset] << 8) | (data[2 + offset] << 16));
                sum += (v1 + FarshHashConstants.FARSH_KEYS[keyindex]) * (ulong)(v2 + FarshHashConstants.FARSH_KEYS[keyindex + 1]);
                break;
            case 6:
                v1 = Utilities.Read32(data, offset);
                offset += 4;
                v2 = Utilities.Read16(data, offset);
                sum += (v1 + FarshHashConstants.FARSH_KEYS[keyindex]) * (ulong)(v2 + FarshHashConstants.FARSH_KEYS[keyindex + 1]);
                break;
            case 5:
                v1 = Utilities.Read32(data, offset);
                offset += 4;
                v2 = data[offset];
                sum += (v1 + FarshHashConstants.FARSH_KEYS[keyindex]) * (ulong)(v2 + FarshHashConstants.FARSH_KEYS[keyindex + 1]);
                break;
            case 4:
                v1 = Utilities.Read32(data, offset);
                sum += (v1 + FarshHashConstants.FARSH_KEYS[keyindex]) * (ulong)FarshHashConstants.FARSH_KEYS[keyindex + 1];
                break;
            case 3:
                v1 = (uint)(data[0 + offset] | (data[1 + offset] << 8) | (data[2 + offset] << 16));
                sum += (v1 + FarshHashConstants.FARSH_KEYS[keyindex]) * (ulong)FarshHashConstants.FARSH_KEYS[keyindex + 1];
                break;
            case 2:
                v1 = Utilities.Read16(data, offset);
                sum += (v1 + FarshHashConstants.FARSH_KEYS[keyindex]) * (ulong)FarshHashConstants.FARSH_KEYS[keyindex + 1];
                break;
            case 1:
                v1 = data[offset];
                sum += (v1 + FarshHashConstants.FARSH_KEYS[keyindex]) * (ulong)FarshHashConstants.FARSH_KEYS[keyindex + 1];
                break;
        }

        return sum;
    }

    public static ulong ComputeHash(byte[] data, ulong seed = 0)
    {
        ulong sum = seed;
        uint bytes = (uint)data.Length;
        uint offset = 0;

        while (bytes >= FarshHashConstants.STRIPE)
        {
            ulong h = farsh_full_block(data, offset);
            sum = farsh_combine(sum, h);
            offset += FarshHashConstants.STRIPE;
            bytes -= FarshHashConstants.STRIPE;
        }

        if (bytes > 0)
        {
            ulong h = farsh_partial_block(data, offset);
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