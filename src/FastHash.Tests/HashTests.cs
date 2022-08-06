using System.Runtime.InteropServices;
using System.Text;
using Genbox.FastHash.CityHash;
using Genbox.FastHash.DjbHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FnvHash;
using Genbox.FastHash.MarvinHash;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.WyHash;
using Genbox.FastHash.XxHash;
using Xunit;

namespace Genbox.FastHash.Tests;

public class HashTests
{
    private static readonly byte[] _data = Encoding.ASCII.GetBytes("This is a test!!");
    private static unsafe readonly byte* _ptr;

    static unsafe HashTests()
    {
        _ptr = (byte*)NativeMemory.Alloc((nuint)_data.Length);

        for (int i = 0; i < _data.Length; i++)
            _ptr[i] = _data[i];
    }

    public static IEnumerable<object[]> CreateAlgorithms32()
    {
        yield return new object[] { nameof(CityHash32), () => CityHash32.ComputeHash(_data), new byte[] { 0x2A, 0x81, 0x7A, 0xBF } };
        yield return new object[] { nameof(Djb2Hash32), () => Djb2Hash32.ComputeHash(_data), new byte[] { 0xCE, 0xED, 0x14, 0x36 } };
        yield return new object[] { nameof(FarmHash32), () => FarmHash32.ComputeHash(_data), new byte[] { 0x7F, 0x0F, 0xF1, 0x11 } };
        yield return new object[] { nameof(Fnv1aHash32), () => Fnv1aHash32.ComputeHash(_data), new byte[] { 0xF6, 0x7E, 0xE0, 0x23 } };
        yield return new object[] { nameof(MarvinHash32), () => MarvinHash32.ComputeHash(_data, 42, 43), new byte[] { 173, 40, 191, 34 } };
        yield return new object[] { nameof(Murmur3Hash32), () => Murmur3Hash32.ComputeHash(_data), new byte[] { 0xF6, 0x08, 0x79, 0x87 } };
        yield return new object[] { nameof(SuperFastHash32), () => SuperFastHash32.ComputeHash(_data), new byte[] { 0x5E, 0xE8, 0x41, 0xB2 } };
        yield return new object[] { nameof(Xx2Hash32), () => Xx2Hash32.ComputeHash(_data), new byte[] { 0x2B, 0xC6, 0xC7, 0x94 } };
    }

    public static IEnumerable<object[]> CreateAlgorithms64()
    {
        yield return new object[] { nameof(CityHash64), () => CityHash64.ComputeHash(_data), new byte[] { 0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A } };
        yield return new object[] { nameof(FarmHash64), () => FarmHash64.ComputeHash(_data), new byte[] { 0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A } };
        yield return new object[] { nameof(SipHash64), () => SipHash64.ComputeHash(_data), new byte[] { 0xBA, 0xFD, 0x2E, 0x42, 0x7E, 0x63, 0x22, 0x97 } };
        yield return new object[] { nameof(Wy3Hash64), () => Wy3Hash64.ComputeHash(_data), new byte[] { 0x3F, 0xA2, 0x72, 0x2A, 0x57, 0x74, 0x52, 0xC2 } };
        yield return new object[] { nameof(Xx2Hash64), () => Xx2Hash64.ComputeHash(_data), new byte[] { 0x75, 0xE4, 0xA8, 0xAF, 0x3C, 0x82, 0xBB, 0xDE } };
        yield return new object[] { nameof(Xx3Hash64), () => Xx3Hash64.ComputeHash(_data), new byte[] { 0xBF, 0x39, 0xFF, 0xB1, 0xB7, 0xF4, 0x3B, 0xC3 } };
    }

    public static IEnumerable<object[]> CreateAlgorithms128()
    {
        yield return new object[] { nameof(CityHash128), () => CityHash128.ComputeHash(_data), new byte[] { 202, 69, 230, 157, 197, 169, 55, 252, 230, 47, 35, 189, 238, 9, 60, 154 } };
        yield return new object[] { nameof(Murmur3Hash128), () => Murmur3Hash128.ComputeHash(_data), new byte[] { 0x79, 0xD6, 0xD4, 0xB7, 0x14, 0x84, 0x73, 0x89, 0x08, 0x3D, 0x39, 0xFD, 0xB7, 0x53, 0xBF, 0x67 } };
        yield return new object[] { nameof(Xx3Hash128), () => Xx3Hash128.ComputeHash(_data), new byte[] { 106, 215, 124, 20, 15, 9, 111, 192, 223, 172, 108, 92, 53, 155, 47, 19 } };
    }

    public static unsafe ICollection<object[]> CreateAlgorithmsUnsafe32()
    {
        return new[]
        {
            new object[] { nameof(CityHash32Unsafe), () => CityHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x2A, 0x81, 0x7A, 0xBF } },
            new object[] { nameof(Djb2Hash32Unsafe), () => Djb2Hash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xCE, 0xED, 0x14, 0x36 } },
            new object[] { nameof(FarmHash32Unsafe), () => FarmHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x7F, 0x0F, 0xF1, 0x11 } },
            new object[] { nameof(Fnv1aHash32Unsafe), () => Fnv1aHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xF6, 0x7E, 0xE0, 0x23 } },
            new object[] { nameof(Murmur3Hash32Unsafe), () => Murmur3Hash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xF6, 0x08, 0x79, 0x87 } },
            new object[] { nameof(SuperFastHash32Unsafe), () => SuperFastHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x5E, 0xE8, 0x41, 0xB2 } },
            new object[] { nameof(Xx2Hash32Unsafe), () => Xx2Hash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x2B, 0xC6, 0xC7, 0x94 } }
        };
    }

    public static unsafe ICollection<object[]> CreateAlgorithmsUnsafe64()
    {
        return new[]
        {
            new object[] { nameof(CityHash64Unsafe), () => CityHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A } },
            new object[] { nameof(FarmHash64Unsafe), () => FarmHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A } },
            new object[] { nameof(SipHash64Unsafe), () => SipHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xBA, 0xFD, 0x2E, 0x42, 0x7E, 0x63, 0x22, 0x97 } },
            new object[] { nameof(Wy3Hash64Unsafe), () => Wy3Hash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x3F, 0xA2, 0x72, 0x2A, 0x57, 0x74, 0x52, 0xC2 } },
            new object[] { nameof(Xx2Hash64Unsafe), () => Xx2Hash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x75, 0xE4, 0xA8, 0xAF, 0x3C, 0x82, 0xBB, 0xDE } },
            new object[] { nameof(Xx3Hash64Unsafe), () => Xx3Hash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xBF, 0x39, 0xFF, 0xB1, 0xB7, 0xF4, 0x3B, 0xC3 } }
        };
    }

    public static unsafe ICollection<object[]> CreateAlgorithmsUnsafe128()
    {
        return new[]
        {
            new object[] { nameof(CityHash128Unsafe), () => CityHash128Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 202, 69, 230, 157, 197, 169, 55, 252, 230, 47, 35, 189, 238, 9, 60, 154 } },
            new object[] { nameof(Murmur3Hash128Unsafe), () => Murmur3Hash128Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x79, 0xD6, 0xD4, 0xB7, 0x14, 0x84, 0x73, 0x89, 0x08, 0x3D, 0x39, 0xFD, 0xB7, 0x53, 0xBF, 0x67 } },
            new object[] { nameof(Xx3Hash128Unsafe), () => Xx3Hash128Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 106, 215, 124, 20, 15, 9, 111, 192, 223, 172, 108, 92, 53, 155, 47, 19 } }
        };
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithms32))]
    public void Check32(string _, Func<uint> func, byte[] expected) => Assert.Equal(expected, BitConverter.GetBytes(func()));

    [Theory]
    [MemberData(nameof(CreateAlgorithms64))]
    public void Check64(string _, Func<ulong> func, byte[] expected) => Assert.Equal(expected, BitConverter.GetBytes(func()));

    [Theory]
    [MemberData(nameof(CreateAlgorithms128))]
    public void Check128(string _, Func<Uint128> func, byte[] expected)
    {
        Uint128 val = func();
        Assert.Equal(BitConverter.ToUInt64(expected), val.Low);
        Assert.Equal(BitConverter.ToUInt64(expected, 8), val.High);
    }

    [Theory]
    [MemberData(nameof(CreateAlgorithmsUnsafe32))]
    public void CheckUnsafe32(string _, Func<uint> func, byte[] expected) => Assert.Equal(expected, BitConverter.GetBytes(func()));

    [Theory]
    [MemberData(nameof(CreateAlgorithmsUnsafe64))]
    public void CheckUnsafe64(string _, Func<ulong> func, byte[] expected) => Assert.Equal(expected, BitConverter.GetBytes(func()));

    [Theory]
    [MemberData(nameof(CreateAlgorithmsUnsafe128))]
    public void CheckUnsafe128(string _, Func<Uint128> func, byte[] expected)
    {
        Uint128 val = func();
        Assert.Equal(BitConverter.ToUInt64(expected), val.Low);
        Assert.Equal(BitConverter.ToUInt64(expected, 8), val.High);
    }
}