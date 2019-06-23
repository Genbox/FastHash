/*
   SipHash reference C implementation
   Copyright (c) 2012-2014 Jean-Philippe Aumasson <jeanphilippe.aumasson@gmail.com>
   Copyright (c) 2012-2014 Daniel J. Bernstein <djb@cr.yp.to>

   To the extent possible under law, the author(s) have dedicated all copyright
   and related and neighboring rights to this software to the public domain
   worldwide. This software is distributed without any warranty.
   You should have received a copy of the CC0 Public Domain Dedication along
   with
   this software. If not, see
   <http://creativecommons.org/publicdomain/zero/1.0/>.
 */

//Ported to C# by Ian Qvist
//Website: https://131002.net/siphash/
//Source: https://github.com/veorq/SipHash

using System.Runtime.CompilerServices;

namespace FastHashesNet.SipHash
{
    public static class SipHash64Unsafe
    {
        public static unsafe ulong ComputeHash(byte* data, int length, ulong seed0 = 0, ulong seed1 = 0, byte cRounds = 2, byte dRounds = 4)
        {
            ulong v0 = SipHashConstants.v0Init;
            ulong v1 = SipHashConstants.v1Init;
            ulong v2 = SipHashConstants.v2Init;
            ulong v3 = SipHashConstants.v3Init;

            int left = length & 7;
            ulong b = (ulong)length << 56;
            int num = length / 8;
            int offset1 = length - left;
            int i;

            v3 ^= seed1;
            v2 ^= seed0;
            v1 ^= seed1;
            v0 ^= seed0;

            ulong* pInput = (ulong*)data;
            ulong* pEnd = pInput + num;

            while (pInput != pEnd)
            {
                ulong m = *pInput++;
                v3 ^= m;

                for (i = 0; i < cRounds; ++i)
                    SipRound(ref v0, ref v1, ref v2, ref v3);

                v0 ^= m;
            }

            switch (left)
            {
                case 7:
                    b |= (ulong)data[6 + offset1] << 48;
                    goto case 6;
                case 6:
                    b |= (ulong)data[5 + offset1] << 40;
                    goto case 5;
                case 5:
                    b |= (ulong)data[4 + offset1] << 32;
                    goto case 4;
                case 4:
                    b |= (ulong)data[3 + offset1] << 24;
                    goto case 3;
                case 3:
                    b |= (ulong)data[2 + offset1] << 16;
                    goto case 2;
                case 2:
                    b |= (ulong)data[1 + offset1] << 8;
                    goto case 1;
                case 1:
                    b |= data[0 + offset1];
                    break;
                case 0:
                    break;
            }

            v3 ^= b;

            for (i = 0; i < cRounds; ++i)
                SipRound(ref v0, ref v1, ref v2, ref v3);

            v0 ^= b;
            v2 ^= 0xFF;

            for (i = 0; i < dRounds; ++i)
                SipRound(ref v0, ref v1, ref v2, ref v3);

            return v0 ^ v1 ^ v2 ^ v3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SipRound(ref ulong v0, ref ulong v1, ref ulong v2, ref ulong v3)
        {
            v0 += v1;
            v1 = Utilities.Rotate(v1, 13);
            v1 ^= v0;
            v0 = Utilities.Rotate(v0, 32);

            v2 += v3;
            v3 = Utilities.Rotate(v3, 16);
            v3 ^= v2;

            v2 += v1;
            v1 = Utilities.Rotate(v1, 17);
            v1 ^= v2;
            v2 = Utilities.Rotate(v2, 32);

            v0 += v3;
            v3 = Utilities.Rotate(v3, 21);
            v3 ^= v0;
        }
    }
}