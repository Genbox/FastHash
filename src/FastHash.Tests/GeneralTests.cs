using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Genbox.FastHash.DJBHash;
using Genbox.FastHash.FarmHash;
using Genbox.FastHash.FNVHash;
using Genbox.FastHash.Marvin;
using Genbox.FastHash.MurmurHash;
using Genbox.FastHash.SipHash;
using Genbox.FastHash.SuperFastHash;
using Genbox.FastHash.xxHash;
using Xunit;

namespace Genbox.FastHash.Tests;

public class GeneralTests
{
    //Test if offset works
    //Test if length works
    //Test against simple test data

    private static readonly byte[] _data = Encoding.ASCII.GetBytes("This is a test!!");
    private static unsafe readonly byte* _ptr;

    static unsafe GeneralTests()
    {
        _ptr = (byte*)NativeMemory.Alloc((nuint)_data.Length);

        for (int i = 0; i < _data.Length; i++)
        {
            _ptr[i] = _data[i];
        }
    }

    public static IEnumerable<object[]> CreateAlgorithms32()
    {
        yield return new object[] { nameof(DJBHash32), () => DJBHash32.ComputeHash(_data), new byte[] { 0xCE, 0xED, 0x14, 0x36 } };
        yield return new object[] { nameof(FarmHash32), () => FarmHash32.ComputeHash(_data), new byte[] { 0x7F, 0x0F, 0xF1, 0x11 } };
        yield return new object[] { nameof(FNV1A32), () => FNV1A32.ComputeHash(_data), new byte[] { 0xF6, 0x7E, 0xE0, 0x23 } };
        yield return new object[] { nameof(Marvin32), () => Marvin32.ComputeHash(_data, 42, 43), new byte[] { 173, 40, 191, 34 } };
        yield return new object[] { nameof(MurmurHash32), () => MurmurHash32.ComputeHash(_data), new byte[] { 0xF6, 0x08, 0x79, 0x87 } };
        yield return new object[] { nameof(SuperFastHash32), () => SuperFastHash32.ComputeHash(_data), new byte[] { 0x5E, 0xE8, 0x41, 0xB2 } };
        yield return new object[] { nameof(xx2Hash32), () => xx2Hash32.ComputeHash(_data), new byte[] { 0x2B, 0xC6, 0xC7, 0x94 } };
    }

    public static IEnumerable<object[]> CreateAlgorithms64()
    {
        yield return new object[] { nameof(FarmHash64), () => FarmHash64.ComputeHash(_data), new byte[] { 0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A } };
        yield return new object[] { nameof(SipHash64), () => SipHash64.ComputeHash(_data), new byte[] { 0xBA, 0xFD, 0x2E, 0x42, 0x7E, 0x63, 0x22, 0x97 } };
        yield return new object[] { nameof(xx2Hash64), () => xx2Hash64.ComputeHash(_data), new byte[] { 0x75, 0xE4, 0xA8, 0xAF, 0x3C, 0x82, 0xBB, 0xDE } };
    }

    public static IEnumerable<object[]> CreateAlgorithms128()
    {
        yield return new object[] { nameof(MurmurHash128), () => MurmurHash128.ComputeHash(_data), new byte[] { 0x79, 0xD6, 0xD4, 0xB7, 0x14, 0x84, 0x73, 0x89, 0x08, 0x3D, 0x39, 0xFD, 0xB7, 0x53, 0xBF, 0x67 } };
    }

    public static unsafe ICollection<object[]> CreateAlgorithmsUnsafe32()
    {
        return new[]
        {
            new object[] { nameof(DJBHash32Unsafe), () => DJBHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xCE, 0xED, 0x14, 0x36 } },
            new object[] { nameof(FarmHash32Unsafe), () => FarmHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x7F, 0x0F, 0xF1, 0x11 } },
            new object[] { nameof(FNV1A32Unsafe), () => FNV1A32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xF6, 0x7E, 0xE0, 0x23 } },
            new object[] { nameof(MurmurHash32Unsafe), () => MurmurHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xF6, 0x08, 0x79, 0x87 } },
            new object[] { nameof(SuperFastHash32Unsafe), () => SuperFastHash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x5E, 0xE8, 0x41, 0xB2 } },
            new object[] { nameof(xx2Hash32Unsafe), () => xx2Hash32Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x2B, 0xC6, 0xC7, 0x94 } },
        };
    }

    public static unsafe ICollection<object[]> CreateAlgorithmsUnsafe64()
    {
        return new[]
        {
            new object[] { nameof(FarmHash64Unsafe), () => FarmHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A } },
            new object[] { nameof(SipHash64Unsafe), () => SipHash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0xBA, 0xFD, 0x2E, 0x42, 0x7E, 0x63, 0x22, 0x97 } },
            new object[] { nameof(xx2Hash64Unsafe), () => xx2Hash64Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x75, 0xE4, 0xA8, 0xAF, 0x3C, 0x82, 0xBB, 0xDE } },
        };
    }

    public static unsafe ICollection<object[]> CreateAlgorithmsUnsafe128()
    {
        return new[]
        {
            new object[] { nameof(MurmurHash128Unsafe), () => MurmurHash128Unsafe.ComputeHash(_ptr, _data.Length), new byte[] { 0x79, 0xD6, 0xD4, 0xB7, 0x14, 0x84, 0x73, 0x89, 0x08, 0x3D, 0x39, 0xFD, 0xB7, 0x53, 0xBF, 0x67 } },
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

    [Fact]
    public void CheckAllHaveCorrectName()
    {
        foreach (Type type in GetAllTypesOf<HashAlgorithm>())
        {
            Assert.True(type.Name.Contains("32") || type.Name.Contains("64") || type.Name.Contains("128"));
        }
    }

    private IEnumerable<Type> GetAllTypesOf<T>()
    {
        Assembly assembly = typeof(DJBHash32).GetTypeInfo().Assembly;

        foreach (Type type in assembly.GetTypes())
        {
            if (type.GetInterfaces().Any(x => x == typeof(T)))
            {
                yield return type;
            }

            if (type.GetTypeInfo().BaseType == typeof(T))
            {
                yield return type;
            }
        }
    }
}