/*
*  xxHash - Fast Hash algorithm
*  Copyright (C) 2012-2016, Yann Collet
*
*  BSD 2-Clause License (http://www.opensource.org/licenses/bsd-license.php)
*
*  Redistribution and use in source and binary forms, with or without
*  modification, are permitted provided that the following conditions are
*  met:
*
*  * Redistributions of source code must retain the above copyright
*  notice, this list of conditions and the following disclaimer.
*  * Redistributions in binary form must reproduce the above
*  copyright notice, this list of conditions and the following disclaimer
*  in the documentation and/or other materials provided with the
*  distribution.
*
*  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
*  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
*  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
*  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
*  OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
*  SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
*  LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
*  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
*  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
*  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
*  OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*
*  You can contact the author at :
*  - xxHash homepage: http://www.xxhash.com
*  - xxHash source repository : https://github.com/Cyan4973/xxHash
*/

//Ported to C# by Ian Qvist
//Source: http://cyan4973.github.io/xxHash/

using System.Runtime.CompilerServices;

namespace Genbox.FastHash.xxHash;

public static class Xx2Hash64Unsafe
{
    public static unsafe ulong ComputeHash(byte* data, int length, uint seed = 0)
    {
        ulong h64;

        if (length >= 32)
        {
            byte* bEnd = data + length;
            byte* limit = bEnd - 31;

            ulong v1 = seed + XxHashConstants.PRIME64_1 + XxHashConstants.PRIME64_2;
            ulong v2 = seed + XxHashConstants.PRIME64_2;
            ulong v3 = seed + 0;
            ulong v4 = seed - XxHashConstants.PRIME64_1;

            do
            {
                v1 = Round(v1, Utilities.Read64(data));
                data += 8;
                v2 = Round(v2, Utilities.Read64(data));
                data += 8;
                v3 = Round(v3, Utilities.Read64(data));
                data += 8;
                v4 = Round(v4, Utilities.Read64(data));
                data += 8;
            } while (data < limit);

            h64 = Utilities.RotateLeft(v1, 1) + Utilities.RotateLeft(v2, 7) + Utilities.RotateLeft(v3, 12) + Utilities.RotateLeft(v4, 18);
            h64 = MergeRound(h64, v1);
            h64 = MergeRound(h64, v2);
            h64 = MergeRound(h64, v3);
            h64 = MergeRound(h64, v4);
        }
        else
            h64 = seed + XxHashConstants.PRIME64_5;

        h64 += (uint)length;

        length &= 31;
        while (length >= 8)
        {
            ulong k1 = Round(0, Utilities.Read64(data));
            data += 8;
            h64 ^= k1;
            h64 = Utilities.RotateLeft(h64, 27) * XxHashConstants.PRIME64_1 + XxHashConstants.PRIME64_4;
            length -= 8;
        }

        if (length >= 4)
        {
            h64 ^= Utilities.Read32(data) * XxHashConstants.PRIME64_1;
            data += 4;
            h64 = Utilities.RotateLeft(h64, 23) * XxHashConstants.PRIME64_2 + XxHashConstants.PRIME64_3;
            length -= 4;
        }

        while (length > 0)
        {
            h64 ^= Utilities.Read8(data) * XxHashConstants.PRIME64_5;
            data++;
            h64 = Utilities.RotateLeft(h64, 11) * XxHashConstants.PRIME64_1;
            length--;
        }

        h64 ^= h64 >> 33;
        h64 *= XxHashConstants.PRIME64_2;
        h64 ^= h64 >> 29;
        h64 *= XxHashConstants.PRIME64_3;
        h64 ^= h64 >> 32;

        return h64;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Round(ulong acc, ulong input)
    {
        acc += input * XxHashConstants.PRIME64_2;
        acc = Utilities.RotateLeft(acc, 31);
        acc *= XxHashConstants.PRIME64_1;
        return acc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong MergeRound(ulong acc, ulong val)
    {
        val = Round(0, val);
        acc ^= val;
        acc = acc * XxHashConstants.PRIME64_1 + XxHashConstants.PRIME64_4;
        return acc;
    }
}