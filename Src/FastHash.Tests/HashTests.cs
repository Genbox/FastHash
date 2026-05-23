using System.Runtime.InteropServices;
using Genbox.FastHash.AesniHash;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FarshHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.FoldHash;
using Genbox.FastHash.GxHash;
using Genbox.FastHash.HighwayHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.MeowHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.PolymurHash;
using Genbox.FastHash.RapidHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.T1haHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Tests;

public class HashTests
{
    private static readonly byte[] _data = "This is a test!!"u8.ToArray();
    private static unsafe readonly byte* _ptr;

    static unsafe HashTests()
    {
        _ptr = (byte*)NativeMemory.Alloc((nuint)_data.Length);

        for (int i = 0; i < _data.Length; i++)
            _ptr[i] = _data[i];
    }

    public static IEnumerable<object[]> CreateAlgorithms32()
    {
        yield return [nameof(CityHash32), () => CityHash32.ComputeHash(_data), new byte[] { 0x2A, 0x81, 0x7A, 0xBF }];
        yield return [nameof(Djb2AltHash32), () => Djb2AltHash32.ComputeHash(_data), new byte[] { 0xCE, 0xED, 0x14, 0x36 }];
        yield return [nameof(Djb2Hash32), () => Djb2Hash32.ComputeHash(_data), new byte[] { 0x9C, 0x17, 0x3D, 0x75 }];
        yield return [nameof(FarmHash32), () => FarmHash32.ComputeHash(_data), new byte[] { 0x2A, 0x81, 0x7A, 0xBF }];
        yield return [nameof(Fnv1aHash32), () => Fnv1aHash32.ComputeHash(_data), new byte[] { 0xF6, 0x7E, 0xE0, 0x23 }];
        yield return [nameof(Gx2Hash32), () => Gx2Hash32.ComputeHash(_data), new byte[] { 0xC3, 0x3A, 0xB7, 0x2A }];
        yield return [nameof(MarvinHash32), () => MarvinHash32.ComputeHash(_data, 0x2A, 0x2B), new byte[] { 0xAD, 0x28, 0xBF, 0x22 }];
        yield return [nameof(Murmur3Hash32), () => Murmur3Hash32.ComputeHash(_data), new byte[] { 0xF6, 0x08, 0x79, 0x87 }];
        yield return [nameof(SuperFastHash32), () => SuperFastHash32.ComputeHash(_data), new byte[] { 0x5E, 0xE8, 0x41, 0xB2 }];
        yield return [nameof(XxHash32), () => XxHash32.ComputeHash(_data), new byte[] { 0x2B, 0xC6, 0xC7, 0x94 }];
    }

    public static IEnumerable<object[]> CreateAlgorithms64()
    {
        yield return [nameof(AesniHash64), () => AesniHash64.ComputeHash(_data), new byte[] { 0x28, 0x1C, 0x4A, 0x87, 0x26, 0x2B, 0x3E, 0xF6 }];
        yield return [nameof(CityHash64), () => CityHash64.ComputeHash(_data), new byte[] { 0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A }];
        yield return [nameof(Djb2AltHash64), () => Djb2AltHash64.ComputeHash(_data), new byte[] { 0xCE, 0xED, 0x14, 0x36, 0xF9, 0x33, 0x6C, 0x8A }];
        yield return [nameof(Djb2Hash64), () => Djb2Hash64.ComputeHash(_data), new byte[] { 0x9C, 0x17, 0x3D, 0x75, 0xA9, 0x35, 0x63, 0x0E }];
        yield return [nameof(FarshHash64), () => FarshHash64.ComputeHash(_data), new byte[] { 0x81, 0x2D, 0x68, 0xB2, 0xE4, 0x17, 0xBE, 0x05 }];
        yield return [nameof(FarmHash64), () => FarmHash64.ComputeHash(_data), new byte[] { 0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A }];
        yield return [nameof(Fnv1aHash64), () => Fnv1aHash64.ComputeHash(_data), new byte[] { 0xD6, 0x35, 0xE0, 0x8E, 0x10, 0x08, 0x2F, 0x47 }];
        yield return [nameof(FoldHash64), () => FoldHash64.ComputeHash(_data), new byte[] { 0xA5, 0x30, 0x7C, 0x35, 0x3C, 0x68, 0xA7, 0x89 }];
        yield return [nameof(FoldHashQuality64), () => FoldHashQuality64.ComputeHash(_data), new byte[] { 0xF9, 0xAD, 0xBA, 0x70, 0x6F, 0x2C, 0x68, 0x4A }];
        yield return [nameof(Gx2Hash64), () => Gx2Hash64.ComputeHash(_data), new byte[] { 0xC3, 0x3A, 0xB7, 0x2A, 0x79, 0xCC, 0xEB, 0xB5 }];
        yield return [nameof(HighwayHash64), () => HighwayHash64.ComputeHash(_data), new byte[] { 0x63, 0x6C, 0x8E, 0x47, 0x06, 0x37, 0xCF, 0x16 }];
        yield return [nameof(MarvinHash64), () => MarvinHash64.ComputeHash(_data), new byte[] { 0x57, 0x44, 0xD2, 0xD9, 0xA0, 0x67, 0x18, 0xDA }];
        yield return [nameof(MeowHash64), () => MeowHash64.ComputeHash(_data), new byte[] { 0xCE, 0xF5, 0xCC, 0xAB, 0xBD, 0xC1, 0x2E, 0x9E }];
        yield return [nameof(Polymur2Hash64), () => Polymur2Hash64.ComputeHash(_data), new byte[] { 0xD8, 0x4B, 0xE5, 0xDB, 0x3C, 0x80, 0x53, 0x69 }];
        yield return [nameof(Rapid3Hash64), () => Rapid3Hash64.ComputeHash(_data), new byte[] { 0x9F, 0xF4, 0x50, 0x8C, 0x0E, 0xC2, 0xD5, 0x6A }];
        yield return [nameof(Rapid3HashMicro64), () => Rapid3HashMicro64.ComputeHash(_data), new byte[] { 0x9F, 0xF4, 0x50, 0x8C, 0x0E, 0xC2, 0xD5, 0x6A }];
        yield return [nameof(Rapid3HashNano64), () => Rapid3HashNano64.ComputeHash(_data), new byte[] { 0x9F, 0xF4, 0x50, 0x8C, 0x0E, 0xC2, 0xD5, 0x6A }];
        yield return [nameof(SipHash64), () => SipHash64.ComputeHash(_data), new byte[] { 0xBA, 0xFD, 0x2E, 0x42, 0x7E, 0x63, 0x22, 0x97 }];
        yield return [nameof(T1ha2Hash64), () => T1ha2Hash64.ComputeHash(_data), new byte[] { 0xC6, 0x87, 0xF0, 0xA7, 0x0E, 0x1B, 0x29, 0xD7 }];
        yield return [nameof(Wy3Hash64), () => Wy3Hash64.ComputeHash(_data), new byte[] { 0x3F, 0xA2, 0x72, 0x2A, 0x57, 0x74, 0x52, 0xC2 }];
        yield return [nameof(Wy4Hash64), () => Wy4Hash64.ComputeHash(_data), new byte[] { 0xC5, 0x96, 0x5C, 0x8B, 0x33, 0x7D, 0x3F, 0x56 }];
        yield return [nameof(XxHash64), () => XxHash64.ComputeHash(_data), new byte[] { 0x75, 0xE4, 0xA8, 0xAF, 0x3C, 0x82, 0xBB, 0xDE }];
        yield return [nameof(Xx3Hash64), () => Xx3Hash64.ComputeHash(_data), new byte[] { 0xBF, 0x39, 0xFF, 0xB1, 0xB7, 0xF4, 0x3B, 0xC3 }];
    }

    public static IEnumerable<object[]> CreateAlgorithms128()
    {
        yield return [nameof(AesniHash128), () => AesniHash128.ComputeHash(_data), new byte[] { 0x28, 0x1C, 0x4A, 0x87, 0x26, 0x2B, 0x3E, 0xF6, 0xF6, 0x53, 0x23, 0xBE, 0xED, 0xAF, 0xB2, 0x2F }];
        yield return [nameof(CityHash128), () => CityHash128.ComputeHash(_data), new byte[] { 0x18, 0xFE, 0x91, 0x9D, 0xAA, 0xA8, 0xF0, 0x6B, 0x35, 0xDC, 0x63, 0xAF, 0x2D, 0xFA, 0xEA, 0x61 }];
        yield return [nameof(FarmHash128), () => FarmHash128.ComputeHash(_data), new byte[] { 0x18, 0xFE, 0x91, 0x9D, 0xAA, 0xA8, 0xF0, 0x6B, 0x35, 0xDC, 0x63, 0xAF, 0x2D, 0xFA, 0xEA, 0x61 }];
        yield return [nameof(Gx2Hash128), () => Gx2Hash128.ComputeHash(_data), new byte[] { 0xC3, 0x3A, 0xB7, 0x2A, 0x79, 0xCC, 0xEB, 0xB5, 0x87, 0xB9, 0x69, 0x4B, 0xD1, 0x2D, 0xDB, 0xE4 }];
        yield return [nameof(MeowHash128), () => MeowHash128.ComputeHash(_data), new byte[] { 0xCE, 0xF5, 0xCC, 0xAB, 0xBD, 0xC1, 0x2E, 0x9E, 0xE7, 0xDE, 0x17, 0xC5, 0x40, 0x68, 0x4E, 0xAF }];
        yield return [nameof(Murmur3Hash128), () => Murmur3Hash128.ComputeHash(_data), new byte[] { 0x79, 0xD6, 0xD4, 0xB7, 0x14, 0x84, 0x73, 0x89, 0x08, 0x3D, 0x39, 0xFD, 0xB7, 0x53, 0xBF, 0x67 }];
        yield return [nameof(Xx3Hash128), () => Xx3Hash128.ComputeHash(_data), new byte[] { 0x6A, 0xD7, 0x7C, 0x14, 0xF, 0x9, 0x6F, 0xC0, 0xDF, 0xAC, 0x6C, 0x5C, 0x35, 0x9B, 0x2F, 0x13 }];
    }

    public static unsafe ICollection<object[]> CreateAlgorithmsUnsafe32()
    {
        return
        [
            [nameof(CityHash32Unsafe), () => CityHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x2A, 0x81, 0x7A, 0xBF }],
            [nameof(Djb2AltHash32Unsafe), () => Djb2AltHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xCE, 0xED, 0x14, 0x36 }],
            [nameof(Djb2Hash32Unsafe), () => Djb2Hash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x9C, 0x17, 0x3D, 0x75 }],
            [nameof(FarmHash32Unsafe), () => FarmHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x2A, 0x81, 0x7A, 0xBF }],
            [nameof(Fnv1aHash32Unsafe), () => Fnv1aHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xF6, 0x7E, 0xE0, 0x23 }],
            [nameof(Murmur3Hash32Unsafe), () => Murmur3Hash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xF6, 0x08, 0x79, 0x87 }],
            [nameof(SuperFastHash32Unsafe), () => SuperFastHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x5E, 0xE8, 0x41, 0xB2 }],
            [nameof(XxHash32Unsafe), () => XxHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x2B, 0xC6, 0xC7, 0x94 }]
        ];
    }

    public static unsafe ICollection<object[]> CreateAlgorithmsUnsafe64()
    {
        return
        [
            [nameof(CityHash64Unsafe), () => CityHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A }],
            [nameof(Djb2AltHash64Unsafe), () => Djb2AltHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xCE, 0xED, 0x14, 0x36, 0xF9, 0x33, 0x6C, 0x8A }],
            [nameof(Djb2Hash64Unsafe), () => Djb2Hash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x9C, 0x17, 0x3D, 0x75, 0xA9, 0x35, 0x63, 0x0E }],
            [nameof(FarshHash64Unsafe), () => FarshHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x81, 0x2D, 0x68, 0xB2, 0xE4, 0x17, 0xBE, 0x05 }],
            [nameof(FarmHash64Unsafe), () => FarmHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A }],
            [nameof(Fnv1aHash64Unsafe), () => Fnv1aHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xD6, 0x35, 0xE0, 0x8E, 0x10, 0x08, 0x2F, 0x47 }],
            [nameof(HighwayHash64Unsafe), () => HighwayHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x63, 0x6C, 0x8E, 0x47, 0x06, 0x37, 0xCF, 0x16 }],
            [nameof(MeowHash64Unsafe), () => MeowHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xCE, 0xF5, 0xCC, 0xAB, 0xBD, 0xC1, 0x2E, 0x9E }],
            [nameof(SipHash64Unsafe), () => SipHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xBA, 0xFD, 0x2E, 0x42, 0x7E, 0x63, 0x22, 0x97 }],
            [nameof(Wy3Hash64Unsafe), () => Wy3Hash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x3F, 0xA2, 0x72, 0x2A, 0x57, 0x74, 0x52, 0xC2 }],
            [nameof(Wy4Hash64Unsafe), () => Wy4Hash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xC5, 0x96, 0x5C, 0x8B, 0x33, 0x7D, 0x3F, 0x56 }],
            [nameof(XxHash64Unsafe), () => XxHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x75, 0xE4, 0xA8, 0xAF, 0x3C, 0x82, 0xBB, 0xDE }],
            [nameof(Xx3Hash64Unsafe), () => Xx3Hash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xBF, 0x39, 0xFF, 0xB1, 0xB7, 0xF4, 0x3B, 0xC3 }]
        ];
    }

    public static unsafe ICollection<object[]> CreateAlgorithmsUnsafe128()
    {
        return
        [
            [nameof(CityHash128Unsafe), () => CityHash128Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x18, 0xFE, 0x91, 0x9D, 0xAA, 0xA8, 0xF0, 0x6B, 0x35, 0xDC, 0x63, 0xAF, 0x2D, 0xFA, 0xEA, 0x61 }],
            [nameof(FarmHash128Unsafe), () => FarmHash128Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x18, 0xFE, 0x91, 0x9D, 0xAA, 0xA8, 0xF0, 0x6B, 0x35, 0xDC, 0x63, 0xAF, 0x2D, 0xFA, 0xEA, 0x61 }],
            [nameof(MeowHash128Unsafe), () => MeowHash128Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xCE, 0xF5, 0xCC, 0xAB, 0xBD, 0xC1, 0x2E, 0x9E, 0xE7, 0xDE, 0x17, 0xC5, 0x40, 0x68, 0x4E, 0xAF }],
            [nameof(Murmur3Hash128Unsafe), () => Murmur3Hash128Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x79, 0xD6, 0xD4, 0xB7, 0x14, 0x84, 0x73, 0x89, 0x08, 0x3D, 0x39, 0xFD, 0xB7, 0x53, 0xBF, 0x67 }],
            [nameof(Xx3Hash128Unsafe), () => Xx3Hash128Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x6A, 0xD7, 0x7C, 0x14, 0xF, 0x9, 0x6F, 0xC0, 0xDF, 0xAC, 0x6C, 0x5C, 0x35, 0x9B, 0x2F, 0x13 }]
        ];
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithms32))]
    public void Check32(string _, Func<uint> func, byte[] expected) => Assert.Equal(expected, BitConverter.GetBytes(func()));

    [Theory]
    [MemberData(nameof(CreateAlgorithms64))]
    public void Check64(string _, Func<ulong> func, byte[] expected) => Assert.Equal(expected, BitConverter.GetBytes(func()));

    [Theory]
    [MemberData(nameof(CreateAlgorithms128))]
    public void Check128(string _, Func<UInt128> func, byte[] expected)
    {
        UInt128 val = func();
        UInt128 expectedVal = new UInt128(BitConverter.ToUInt64(expected), BitConverter.ToUInt64(expected, 8));
        Assert.Equal(expectedVal, val);
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithmsUnsafe32))]
    public void CheckUnsafe32(string _, Func<uint> func, byte[] expected) => Assert.Equal(expected, BitConverter.GetBytes(func()));

    [Theory]
    [MemberData(nameof(CreateAlgorithmsUnsafe64))]
    public void CheckUnsafe64(string _, Func<ulong> func, byte[] expected) => Assert.Equal(expected, BitConverter.GetBytes(func()));

    [Theory]
    [MemberData(nameof(CreateAlgorithmsUnsafe128))]
    public void CheckUnsafe128(string _, Func<UInt128> func, byte[] expected)
    {
        UInt128 val = func();
        UInt128 expectedVal = new UInt128(BitConverter.ToUInt64(expected), BitConverter.ToUInt64(expected, 8));

        Assert.Equal(expectedVal, val);
    }
}