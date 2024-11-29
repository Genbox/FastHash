using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Genbox.FastHash.MarvinHash;

public static class MarvinHash32
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint p0 = input;
        uint p1 = RotateLeft(input, 20);

        p1 += p0;
        p0 = RotateLeft(p0, 9);

        p0 ^= p1;
        p1 = RotateLeft(p1, 27);

        p1 += p0;
        p0 = RotateLeft(p0, 19);

        p1 += 128;

        p0 ^= p1;
        p1 = RotateLeft(p1, 20);

        p1 += p0;
        p0 = RotateLeft(p0, 9);

        p0 ^= p1;
        p1 = RotateLeft(p1, 27);

        p1 += p0;
        p0 = RotateLeft(p0, 19);

        p0 ^= p1;
        p1 = RotateLeft(p1, 20);

        p1 += p0;
        p0 = RotateLeft(p0, 9);

        p0 ^= p1;
        p1 = RotateLeft(p1, 27);

        p1 += p0;
        p0 = RotateLeft(p0, 19);

        return p0 ^ p1;
    }

    public static uint ComputeHash(ReadOnlySpan<byte> data, uint seed1 = 0xb79308cd, uint seed2 = 0xced93cd5)
    {
        MarvinHash64.ComputeHash(ref MemoryMarshal.GetReference(data), (uint)data.Length, ref seed1, ref seed2);
        return seed1 ^ seed2;
    }
}