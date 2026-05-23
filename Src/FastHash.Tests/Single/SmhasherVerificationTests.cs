using System.Buffers.Binary;
using Genbox.FastHash.AesniHash;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FarshHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Tests.Single;

public class SmhasherVerificationTests
{
    [Fact]
    public void XxHash32Verification() => Verify32(XxHash32.ComputeHash, 0xBA88B743U);

    [Fact]
    public void XxHash64Verification() => Verify64(static (data, seed) => XxHash64.ComputeHash(data, seed), 0x024B7CF4U);

    [Fact]
    public void CityHash32Verification() => Verify32(CityHash32.ComputeHash, 0xEDED9084U);

    [Fact]
    public void CityHash64NoSeedVerification() => Verify64(static (data, _) => CityHash64.ComputeHash(data), 0x4C4E54B1U);

    [Fact]
    public void CityHash64Verification() => Verify64(static (data, seed) => CityHash64.ComputeHash(data, seed), 0x5FABC5C5U);

    [Fact]
    public void CityHash128Verification() => Verify128(static (data, seed) => CityHash128.ComputeHash(data, new UInt128(seed, 0)), 0x305C0D9AU);

    [Fact]
    public void FarmHash64Verification() => Verify64(static (data, seed) => FarmHash64.ComputeHash(data, seed), 0xEBC4A679U);

    [Fact]
    public void FarmHash128Verification() => Verify128(static (data, seed) => FarmHash128.ComputeHash(data, new UInt128(seed, 0)), 0x305C0D9AU);

    [Fact]
    public void FarshHash64Verification() => Verify64(static (data, seed) => FarshHash64.ComputeHash(data, seed), 0xDE2FDAEEU);

    [Fact]
    public void Murmur3Hash32Verification() => Verify32(Murmur3Hash32.ComputeHash, 0xB0F57EE3U);

    [Fact]
    public void Murmur3Hash128Verification() => Verify128(Murmur3Hash128.ComputeHash, 0x6384BA69U);

    [Fact]
    public void SipHash64Verification() => Verify64(static (data, seed) => data.IsEmpty ? 0 : SipHash64.ComputeHash(data, seed), 0xC58D7F9CU);

    [Fact]
    public void SuperFastHash32Verification() => Verify32(SuperFastHash32.ComputeHash, 0x6306A6FEU);

    [Fact]
    public void AesniHash64Verification()
    {
        if (!AesniHash64.IsSupported)
            return;

        Verify64(AesniHash64.ComputeHash, 0x3AA1A480U);
    }

    [Fact]
    public void AesniHash128Verification()
    {
        if (!AesniHash128.IsSupported)
            return;

        Verify128(AesniHash128.ComputeHash, 0xF06DA1B1U);
    }

    // Not included: SMHasher's xxh3 output is from an older XXH3 variant,
    // rapidhash.txt is rapidhash v1 while FastHash implements v3, and FarmHash32
    // has no authoritative non-zero verification value in SMHasher's main.cpp.

    private static void Verify32(Hash32 hash, uint expected)
    {
        Span<byte> key = stackalloc byte[256];
        Span<byte> hashes = stackalloc byte[sizeof(uint) * 256];

        for (int i = 0; i < 256; i++)
        {
            key[i] = (byte)i;
            BinaryPrimitives.WriteUInt32LittleEndian(hashes[(i * sizeof(uint))..], hash(key[..i], (uint)(256 - i)));
        }

        Assert.Equal(expected, hash(hashes, 0));
    }

    private static void Verify64(Hash64 hash, uint expected)
    {
        Span<byte> key = stackalloc byte[256];
        Span<byte> hashes = stackalloc byte[sizeof(ulong) * 256];

        for (int i = 0; i < 256; i++)
        {
            key[i] = (byte)i;
            BinaryPrimitives.WriteUInt64LittleEndian(hashes[(i * sizeof(ulong))..], hash(key[..i], (uint)(256 - i)));
        }

        Assert.Equal(expected, unchecked((uint)hash(hashes, 0)));
    }

    private static void Verify128(Hash128 hash, uint expected)
    {
        Span<byte> key = stackalloc byte[256];
        Span<byte> hashes = stackalloc byte[16 * 256];

        for (int i = 0; i < 256; i++)
        {
            key[i] = (byte)i;
            UInt128 value = hash(key[..i], (uint)(256 - i));
            Span<byte> output = hashes[(i * 16)..];
            BinaryPrimitives.WriteUInt64LittleEndian(output, value.Low);
            BinaryPrimitives.WriteUInt64LittleEndian(output[sizeof(ulong)..], value.High);
        }

        Assert.Equal(expected, unchecked((uint)hash(hashes, 0).Low));
    }

    private delegate uint Hash32(ReadOnlySpan<byte> data, uint seed);
    private delegate ulong Hash64(ReadOnlySpan<byte> data, uint seed);
    private delegate UInt128 Hash128(ReadOnlySpan<byte> data, uint seed);
}