using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Genbox.FastHash.MarvinHash;

public static class MarvinHash32
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input)
    {
        uint seed1 = 0xb79308cd;
        uint seed2 = 0xced93cd5;

        seed1 += input;
        MarvinHash64.Block(ref seed1, ref seed2);
        seed1 += 0x80;
        MarvinHash64.Block(ref seed1, ref seed2);
        MarvinHash64.Block(ref seed1, ref seed2);
        return seed1 ^ seed2;
    }

    public static uint ComputeHash(ReadOnlySpan<byte> data, uint seed1 = 0xb79308cd, uint seed2 = 0xced93cd5)
    {
        MarvinHash64.ComputeHash(ref MemoryMarshal.GetReference(data), (uint)data.Length, ref seed1, ref seed2);
        return seed1 ^ seed2;
    }
}