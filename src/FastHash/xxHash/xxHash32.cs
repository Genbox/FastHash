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

public static class xxHash32
{
    public static uint ComputeHash(byte[] data, uint seed = 0)
    {
        uint bEnd = (uint)data.Length;
        uint h32;
        uint offset = 0;

        if (bEnd >= 16)
        {
            uint limit = bEnd - 16;
            uint v1 = seed + xxHashConstants.PRIME32_1 + xxHashConstants.PRIME32_2;
            uint v2 = seed + xxHashConstants.PRIME32_2;
            uint v3 = seed + 0;
            uint v4 = seed - xxHashConstants.PRIME32_1;

            do
            {
                v1 = Round(v1, Utilities.Fetch32(data, offset));
                offset += 4;
                v2 = Round(v2, Utilities.Fetch32(data, offset));
                offset += 4;
                v3 = Round(v3, Utilities.Fetch32(data, offset));
                offset += 4;
                v4 = Round(v4, Utilities.Fetch32(data, offset));
                offset += 4;
            } while (offset <= limit);

            h32 = Utilities.RotateLeft(v1, 1) + Utilities.RotateLeft(v2, 7) + Utilities.RotateLeft(v3, 12) + Utilities.RotateLeft(v4, 18);
        }
        else
            h32 = seed + xxHashConstants.PRIME32_5;

        h32 += bEnd;

        while (offset + 4 <= bEnd)
        {
            h32 += Utilities.Fetch32(data, offset) * xxHashConstants.PRIME32_3;
            h32 = Utilities.RotateLeft(h32, 17) * xxHashConstants.PRIME32_4;
            offset += 4;
        }

        while (offset < bEnd)
        {
            h32 += data[offset] * xxHashConstants.PRIME32_5;
            h32 = Utilities.RotateLeft(h32, 11) * xxHashConstants.PRIME32_1;
            offset++;
        }

        h32 ^= h32 >> 15;
        h32 *= xxHashConstants.PRIME32_2;
        h32 ^= h32 >> 13;
        h32 *= xxHashConstants.PRIME32_3;
        h32 ^= h32 >> 16;

        return h32;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Round(uint seed, uint input)
    {
        seed += input * xxHashConstants.PRIME32_2;
        seed = Utilities.RotateLeft(seed, 13);
        seed *= xxHashConstants.PRIME32_1;
        return seed;
    }
}