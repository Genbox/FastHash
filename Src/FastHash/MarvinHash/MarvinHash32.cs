using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Genbox.FastHash.MarvinHash;

public static class MarvinHash32
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input) => ComputeIndex(input, 0xb79308cd, 0xced93cd5);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ComputeIndex(uint input, uint seed1, uint seed2)
    {
        if (!BitConverter.IsLittleEndian)
            input = ByteSwap(input);

        seed1 += input;
        MarvinHash64.Block(ref seed1, ref seed2);
        seed1 += BitConverter.IsLittleEndian ? 0x80u : 0x8000_0000u;
        MarvinHash64.Block(ref seed1, ref seed2);
        MarvinHash64.Block(ref seed1, ref seed2);
        return seed1 ^ seed2;
    }

    public static uint ComputeHash(ReadOnlySpan<byte> data) => ComputeHash(data, 0xb79308cd, 0xced93cd5);

    public static uint ComputeHash(ReadOnlySpan<byte> data, uint seed1, uint seed2)
    {
        MarvinHash64.ComputeHash(ref MemoryMarshal.GetReference(data), (uint)data.Length, ref seed1, ref seed2);
        return seed1 ^ seed2;
    }
}