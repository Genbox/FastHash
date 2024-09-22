using Genbox.FastHash.HighwayHash;

namespace Genbox.FastHash.Tests.Single;

public class HighwayHashTests
{
    private const uint kMaxSize = 64;

    private static readonly ulong[] _testKeys =
    [
        0x0706050403020100UL, 0x0F0E0D0C0B0A0908UL,
        0x1716151413121110UL, 0x1F1E1D1C1B1A1918UL
    ];

    private static readonly ulong[] _testKeys2 =
    [
        1ul, 2ul,
        3ul, 4ul
    ];

    private readonly ulong[] Expected64 =
    [
        0x907A56DE22C26E53ul, 0x7EAB43AAC7CDDD78ul, 0xB8D0569AB0B53D62ul,
        0x5C6BEFAB8A463D80ul, 0xF205A46893007EDAul, 0x2B8A1668E4A94541ul,
        0xBD4CCC325BEFCA6Ful, 0x4D02AE1738F59482ul, 0xE1205108E55F3171ul,
        0x32D2644EC77A1584ul, 0xF6E10ACDB103A90Bul, 0xC3BBF4615B415C15ul,
        0x243CC2040063FA9Cul, 0xA89A58CE65E641FFul, 0x24B031A348455A23ul,
        0x40793F86A449F33Bul, 0xCFAB3489F97EB832ul, 0x19FE67D2C8C5C0E2ul,
        0x04DD90A69C565CC2ul, 0x75D9518E2371C504ul, 0x38AD9B1141D3DD16ul,
        0x0264432CCD8A70E0ul, 0xA9DB5A6288683390ul, 0xD7B05492003F028Cul,
        0x205F615AEA59E51Eul, 0xEEE0C89621052884ul, 0x1BFC1A93A7284F4Ful,
        0x512175B5B70DA91Dul, 0xF71F8976A0A2C639ul, 0xAE093FEF1F84E3E7ul,
        0x22CA92B01161860Ful, 0x9FC7007CCF035A68ul, 0xA0C964D9ECD580FCul,
        0x2C90F73CA03181FCul, 0x185CF84E5691EB9Eul, 0x4FC1F5EF2752AA9Bul,
        0xF5B7391A5E0A33EBul, 0xB9B84B83B4E96C9Cul, 0x5E42FE712A5CD9B4ul,
        0xA150F2F90C3F97DCul, 0x7FA522D75E2D637Dul, 0x181AD0CC0DFFD32Bul,
        0x3889ED981E854028ul, 0xFB4297E8C586EE2Dul, 0x6D064A45BB28059Cul,
        0x90563609B3EC860Cul, 0x7AA4FCE94097C666ul, 0x1326BAC06B911E08ul,
        0xB926168D2B154F34ul, 0x9919848945B1948Dul, 0xA2A98FC534825EBEul,
        0xE9809095213EF0B6ul, 0x582E5483707BC0E9ul, 0x086E9414A88A6AF5ul,
        0xEE86B98D20F6743Dul, 0xF89B7FF609B1C0A7ul, 0x4C7D9CC19E22C3E8ul,
        0x9A97005024562A6Ful, 0x5DD41CF423E6EBEFul, 0xDF13609C0468E227ul,
        0x6E0DA4F64188155Aul, 0xB755BA4B50D7D4A1ul, 0x887A3484647479BDul,
        0xAB8EEBE9BF2139A0ul, 0x75542C5D4CD2A6FFul
    ];

    [Fact]
    public void HighwayHash64Test()
    {
        byte[] data = new byte[kMaxSize + 1];
        byte i;
        for (i = 0; i <= kMaxSize; i++)
        {
            data[i] = i;
            TestHash64(Expected64[i], data, i, _testKeys);
        }

        for (i = 0; i < 33; i++)
            data[i] = (byte)(128 + i);
        TestHash64(0x53c516cce478cad7ul, data, 33, _testKeys2);
    }

    private unsafe void TestHash64(ulong expected, byte[] data, int size, ulong[] key)
    {
        fixed (byte* ptr = data)
        {
            ulong hash = HighwayHash64Unsafe.ComputeHash(ptr, size, key[0], key[1], key[2], key[3]);
            Assert.Equal(expected, hash);
        }
    }
}