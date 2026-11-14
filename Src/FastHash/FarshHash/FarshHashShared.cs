// C# port of Bulat-Ziganshin/FARSH 0.2.0 (d74ef3a). Copyright (c) 2015-16 Bulat Ziganshin.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using static Genbox.FastHash.FarshHash.FarshHashConstants;

namespace Genbox.FastHash.FarshHash;

internal static class FarshHashShared
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong farsh_combine(ulong sum, ulong h)
    {
        h *= PRIME64_2;
        h += h >> 31;
        h *= PRIME64_1;
        sum ^= h;
        sum = ((sum + (sum >> 27)) * PRIME64_1) + PRIME64_4;
        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint farsh_final(ulong sum)
    {
        sum ^= sum >> 33;
        sum *= PRIME64_2;
        sum ^= sum >> 29;
        sum *= PRIME64_3;
        return (uint)sum ^ (uint)(sum >> 32);
    }
}