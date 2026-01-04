#if NET8_0_OR_GREATER
using System.Globalization;
using System.Runtime.Intrinsics.X86;
using Genbox.FastHash;
using Genbox.FastHash.MeowHash;

namespace Genbox.FastHash.Tests.Single;

public class MeowHashTests
{
    private readonly record struct Vector(string Name, byte[] Data, string Expected);

    private static readonly byte[] MessageDigest = "message digest"u8.ToArray();
    private static readonly byte[] Sequence256 = CreateSequence256();

    private static readonly Vector[] Vectors =
    [
        new Vector("empty", [], "75A7B555-0383265E-657CA02A-5859C045"),
        new Vector("a", "a"u8.ToArray(), "B26B64E1-346CA4B5-D8BE5296-340E50E3"),
        new Vector("abc", "abc"u8.ToArray(), "5F6D236B-424C9AB4-19F773EC-430ACC3B"),
        new Vector("message digest", MessageDigest, "FDD80026-549F8348-9B0FBD11-3B9C49DF"),
        new Vector("seq256", Sequence256, "C8D5A459-085BA8B7-D3604660-411FE318")
    ];

    [Fact]
    public unsafe void TestVectors()
    {
        if (!Aes.IsSupported || !Sse2.IsSupported || !Ssse3.IsSupported)
            return;

        foreach (Vector vector in Vectors)
        {
            byte[] buffer = vector.Data.Length == 0 ? new byte[1] : vector.Data;

            fixed (byte* ptr = buffer)
            {
                UInt128 hash = MeowHash128Unsafe.ComputeHash(ptr, vector.Data.Length);
                Assert.Equal(ParseMeowHash(vector.Expected), hash);
            }
        }
    }

    private static UInt128 ParseMeowHash(string value)
    {
        string[] parts = value.Split('-');
        uint v3 = uint.Parse(parts[0], NumberStyles.HexNumber);
        uint v2 = uint.Parse(parts[1], NumberStyles.HexNumber);
        uint v1 = uint.Parse(parts[2], NumberStyles.HexNumber);
        uint v0 = uint.Parse(parts[3], NumberStyles.HexNumber);
        ulong low = ((ulong)v1 << 32) | v0;
        ulong high = ((ulong)v3 << 32) | v2;
        return new UInt128(low, high);
    }

    private static byte[] CreateSequence256()
    {
        byte[] data = new byte[256];
        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)i;

        return data;
    }
}
#endif