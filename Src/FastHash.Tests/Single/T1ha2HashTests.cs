using System.Buffers.Binary;
using Genbox.FastHash.T1haHash;

namespace Genbox.FastHash.Tests.Single;

public class T1ha2HashTests
{
    private static readonly byte[] T1haTestPattern =
    [
        0, 1, 2, 3, 4, 5, 6, 7, 0xFF, 0x7F, 0x3F,
        0x1F, 0xF, 8, 16, 32, 64, 0x80, 0xFE, 0xFC, 0xF8, 0xF0,
        0xE0, 0xC0, 0xFD, 0xFB, 0xF7, 0xEF, 0xDF, 0xBF, 0x55, 0xAA, 11,
        17, 19, 23, 29, 37, 42, 43, (byte)'a', (byte)'b', (byte)'c', (byte)'d',
        (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o',
        (byte)'p', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x'
    ];

    private static readonly ulong[] T1haRefval2AtOnce =
    [
        0x0000000000000000UL,
        0x772C7311BE32FF42UL, 0x444753D23F207E03UL, 0x71F6DF5DA3B4F532UL, 0x555859635365F660UL,
        0xE98808F1CD39C626UL, 0x2EB18FAF2163BB09UL, 0x7B9DD892C8019C87UL, 0xE2B1431C4DA4D15AUL,
        0x1984E718A5477F70UL, 0x08DD17B266484F79UL, 0x4C83A05D766AD550UL, 0x92DCEBB131D1907DUL,
        0xD67BC6FC881B8549UL, 0xF6A9886555FBF66BUL, 0x6E31616D7F33E25EUL, 0x36E31B7426E3049DUL,
        0x4F8E4FAF46A13F5FUL, 0x03EB0CB3253F819FUL, 0x636A7769905770D2UL, 0x3ADF3781D16D1148UL,
        0x92D19CB1818BC9C2UL, 0x283E68F4D459C533UL, 0xFA83A8A88DECAA04UL, 0x8C6F00368EAC538CUL,
        0x7B66B0CF3797B322UL, 0x5131E122FDABA3FFUL, 0x6E59FF515C08C7A9UL, 0xBA2C5269B2C377B0UL,
        0xA9D24FD368FE8A2BUL, 0x22DB13D32E33E891UL, 0x7B97DFC804B876E5UL, 0xC598BDFCD0E834F9UL,
        0xB256163D3687F5A7UL, 0x66D7A73C6AEF50B3UL, 0x25A7201C85D9E2A3UL, 0x911573EDA15299AAUL,
        0x5C0062B669E18E4CUL, 0x17734ADE08D54E28UL, 0xFFF036E33883F43BUL, 0xFE0756E7777DF11EUL,
        0x37972472D023F129UL, 0x6CFCE201B55C7F57UL, 0xE019D1D89F02B3E1UL, 0xAE5CC580FA1BB7E6UL,
        0x295695FB7E59FC3AUL, 0x76B6C820A40DD35EUL, 0xB1680A1768462B17UL, 0x2FB6AF279137DADAUL,
        0x28FB6B4366C78535UL, 0xEC278E53924541B1UL, 0x164F8AAB8A2A28B5UL, 0xB6C330AEAC4578ADUL,
        0x7F6F371070085084UL, 0x94DEAD60C0F448D3UL, 0x99737AC232C559EFUL, 0x6F54A6F9CA8EDD57UL,
        0x979B01E926BFCE0CUL, 0xF7D20BC85439C5B4UL, 0x64EDB27CD8087C12UL, 0x11488DE5F79C0BE2UL,
        0x25541DDD1680B5A4UL, 0x8B633D33BE9D1973UL, 0x404A3113ACF7F6C6UL, 0xC59DBDEF8550CD56UL,
        0x039D23C68F4F992CUL, 0x5BBB48E4BDD6FD86UL, 0x41E312248780DF5AUL, 0xD34791CE75D4E94FUL,
        0xED523E5D04DCDCFFUL, 0x7A6BCE0B6182D879UL, 0x21FB37483CAC28D8UL, 0x19A1B66E8DA878ADUL,
        0x6F804C5295B09ABEUL, 0x2A4BE5014115BA81UL, 0xA678ECC5FC924BE0UL, 0x50F7A54A99A36F59UL,
        0x0FD7E63A39A66452UL, 0x5AB1B213DD29C4E4UL, 0xF3ED80D9DF6534C5UL, 0xC736B12EF90615FDUL
    ];

    [Fact]
    public void T1ha2AtOnceSelfcheck()
    {
        int index = 0;
        byte[] pattern = T1haTestPattern;

        Assert.Equal(T1haRefval2AtOnce[index++], T1ha2Hash64.ComputeHash(ReadOnlySpan<byte>.Empty));
        Assert.Equal(T1haRefval2AtOnce[index++], T1ha2Hash64.ComputeHash(ReadOnlySpan<byte>.Empty, ulong.MaxValue));
        Assert.Equal(T1haRefval2AtOnce[index++], T1ha2Hash64.ComputeHash(pattern));

        ulong seed = 1;
        for (int i = 1; i < 64; i++)
        {
            Assert.Equal(T1haRefval2AtOnce[index++], T1ha2Hash64.ComputeHash(pattern.AsSpan(0, i), seed));
            seed <<= 1;
        }

        seed = ulong.MaxValue;
        for (int i = 1; i <= 7; i++)
        {
            seed <<= 1;
            Assert.Equal(T1haRefval2AtOnce[index++], T1ha2Hash64.ComputeHash(pattern.AsSpan(i, 64 - i), seed));
        }

        byte[] longPattern = new byte[512];
        for (int i = 0; i < longPattern.Length; i++)
            longPattern[i] = unchecked((byte)i);

        for (int i = 0; i <= 7; i++)
            Assert.Equal(T1haRefval2AtOnce[index++], T1ha2Hash64.ComputeHash(longPattern.AsSpan(i, 128 + (i * 17)), seed));

        Assert.Equal(T1haRefval2AtOnce.Length, index);
    }

    [Fact]
    public void ComputeIndexMatchesByteHash()
    {
        ulong[] inputs =
        [
            0UL,
            1UL,
            0x0123456789abcdefUL,
            12808224424451380151UL,
            ulong.MaxValue
        ];

        Span<byte> data = stackalloc byte[8];

        foreach (ulong input in inputs)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(data, input);

            Assert.Equal(T1ha2Hash64.ComputeHash(data), T1ha2Hash64.ComputeIndex(input));
            Assert.Equal(T1ha2Hash64.ComputeHash(data, 123), T1ha2Hash64.ComputeIndex(input, 123));
        }
    }
}