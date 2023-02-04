using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Tests.Single;

public class Xx2HashTests
{
    private const uint PRIME32 = 2654435761U;
    private const ulong PRIME64 = 11400714785074694797UL;
    private readonly byte[] _sanityBuffer;

    //Source: https://github.com/Cyan4973/xxHash/blob/dev/cli/xsum_sanity_check.c#L99
    private static readonly (int len, uint seed, uint result)[] _testVectors32 =
    {
        (0, 0, 0x02CC5D05U),
        (0, PRIME32, 0x36B78AE7U),
        (1, 0, 0xCF65B03EU),
        (1, PRIME32, 0xB4545AA4U),
        (14, 0, 0x1208E7E2U),
        (14, PRIME32, 0x6AF1D1FEU),
        (222, 0, 0x5BD11DBDU),
        (222, PRIME32, 0x58803C5FU)
    };

    private static readonly (int, uint, ulong)[] _testVectors64 =
    {
        (0, 0, 0xEF46DB3751D8E999UL),
        (0, PRIME32, 0xAC75FDA2929B17EFUL),
        (1, 0, 0xE934A84ADB052768UL),
        (1, PRIME32, 0x5014607643A9B4C3UL),
        (4, 0, 0x9136A0DCA57457EEUL),
        (14, 0, 0x8282DCC4994E35C8UL),
        (14, PRIME32, 0xC3BD6BF63DEB6DF0UL),
        (222, 0, 0xB641AE8CB691C174UL),
        (222, PRIME32, 0x20CB8AB7AE10C14AUL)
    };

    public Xx2HashTests()
    {
        _sanityBuffer = new byte[2367];

        ulong byteGen = PRIME32;
        for (int i = 0; i < _sanityBuffer.Length; i++)
        {
            _sanityBuffer[i] = (byte)(byteGen >> 56);
            byteGen = unchecked(byteGen * PRIME64);
        }
    }

    [Fact]
    public unsafe void Xx2Hash32TestVectors()
    {
        for (int i = 0; i < _testVectors32.Length; i++)
        {
            (int len, uint seed, uint result) = _testVectors32[i];
            Assert.Equal(result, Xx2Hash32.ComputeHash(_sanityBuffer[..len], seed));

            fixed (byte* ptr = _sanityBuffer[..len])
                Assert.Equal(result, Xx2Hash32Unsafe.ComputeHash(ptr, len, seed));
        }
    }

    [Fact]
    public unsafe void Xx2Hash64TestVectors()
    {
        for (int i = 0; i < _testVectors64.Length / 3; i++)
        {
            (int len, uint seed, ulong result) = _testVectors64[i];
            Assert.Equal(Xx2Hash64.ComputeHash(_sanityBuffer[..len], seed), result);

            fixed (byte* ptr = _sanityBuffer[..len])
                Assert.Equal(Xx2Hash64Unsafe.ComputeHash(ptr, len, seed), result);
        }
    }

    [Fact]
    public void Xx2HashIndexTest()
    {
        ulong val = 1ul;
        for (int i = 1; i <= 64; i++)
        {
            ulong h1 = Xx2Hash64.ComputeHash(BitConverter.GetBytes(val));
            ulong h2 = Xx2Hash64.ComputeIndex(val);
            Assert.Equal(h1, h2);

            val <<= 1;
        }
    }
}