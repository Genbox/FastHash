using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using FastHashesNet.DJBHash;
using FastHashesNet.FarmHash;
using FastHashesNet.FNVHash;
using FastHashesNet.MurmurHash;
using FastHashesNet.SipHash;
using FastHashesNet.SuperFastHash;
using FastHashesNet.xxHash;
using Xunit;

namespace FastHashesNet.Tests;

public class GeneralTests
{
    //Test if offset works
    //Test if length works
    //Test against simple test data

    private readonly byte[] _testData = Encoding.ASCII.GetBytes("This is a test!!");

    private readonly Dictionary<Type, byte[]> _testResults = new Dictionary<Type, byte[]>
    {
        { typeof(DJBHash32), new byte[] { 0xCE, 0xED, 0x14, 0x36 } },
        { typeof(FarmHash32), new byte[] { 0x7F, 0x0F, 0xF1, 0x11 } },
        { typeof(FarmHash64), new byte[] { 0x17, 0xEC, 0x34, 0x98, 0x3A, 0xE1, 0xE1, 0x3A } },
        { typeof(FNV1A32), new byte[] { 0xF6, 0x7E, 0xE0, 0x23 } },
        { typeof(MurmurHash32), new byte[] { 0xF6, 0x08, 0x79, 0x87 } },
        { typeof(MurmurHash128), new byte[] { 0x79, 0xD6, 0xD4, 0xB7, 0x14, 0x84, 0x73, 0x89, 0x08, 0x3D, 0x39, 0xFD, 0xB7, 0x53, 0xBF, 0x67 } },
        { typeof(SipHash64), new byte[] { 0xBA, 0xFD, 0x2E, 0x42, 0x7E, 0x63, 0x22, 0x97 } },
        { typeof(SuperFastHash32), new byte[] { 0x5E, 0xE8, 0x41, 0xB2 } },
        { typeof(xxHash32), new byte[] { 0x2B, 0xC6, 0xC7, 0x94 } },
        { typeof(xxHash64), new byte[] { 0x75, 0xE4, 0xA8, 0xAF, 0x3C, 0x82, 0xBB, 0xDE } }
    };

    private string ByteStuff(byte[] data)
    {
        string value = Utilities.ToHex(data);

        List<string> values = new List<string>();
        for (int i = 0; i < value.Length; i += 2)
        {
            values.Add("0x" + value[i] + value[i + 1]);
        }

        string returnVal = string.Join(", ", values);
        return returnVal;
    }

    private IEnumerable<Type> GetAllTypesOf<T>()
    {
        Assembly assembly = typeof(Utilities).GetTypeInfo().Assembly;

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

    [Fact]
    public void CheckReferenceResults()
    {
        //DJBHash32
        uint r1 = DJBHash32.ComputeHash(_testData);
        Assert.Equal(BitConverter.ToUInt32(_testResults[typeof(DJBHash32)], 0), r1);

        //Farmhash32
        uint r2 = FarmHash32.ComputeHash(_testData);
        Assert.Equal(BitConverter.ToUInt32(_testResults[typeof(FarmHash32)], 0), r2);

        //Farmhash64
        ulong r3 = FarmHash64.ComputeHash(_testData);
        Assert.Equal(BitConverter.ToUInt64(_testResults[typeof(FarmHash64)], 0), r3);

        //FNV1A32
        uint r4 = FNV1A32.ComputeHash(_testData);
        Assert.Equal(BitConverter.ToUInt32(_testResults[typeof(FNV1A32)], 0), r4);

        //MurmurHash32
        uint r5 = MurmurHash32.ComputeHash(_testData);
        Assert.Equal(BitConverter.ToUInt32(_testResults[typeof(MurmurHash32)], 0), r5);

        //MurmurHash128
        byte[] r6 = MurmurHash128.ComputeHash(_testData);
        Assert.True(_testResults[typeof(MurmurHash128)].SequenceEqual(r6));

        //SipHash64
        ulong r7 = SipHash64.ComputeHash(_testData);
        Assert.Equal(BitConverter.ToUInt64(_testResults[typeof(SipHash64)], 0), r7);

        //SuperFastHash32
        uint r8 = SuperFastHash32.ComputeHash(_testData);
        Assert.Equal(BitConverter.ToUInt32(_testResults[typeof(SuperFastHash32)], 0), r8);

        //xxHash32
        uint r9 = xxHash32.ComputeHash(_testData);
        Assert.Equal(BitConverter.ToUInt32(_testResults[typeof(xxHash32)], 0), r9);

        //xxHash64
        ulong r10 = xxHash64.ComputeHash(_testData);
        Assert.Equal(BitConverter.ToUInt64(_testResults[typeof(xxHash64)], 0), r10);
    }

    [Fact]
    public void CheckReferenceResultsUnsafe()
    {
        unsafe
        {
            fixed (byte* ptr = _testData)
            {
                //DJBHash32
                uint r1 = DJBHash32Unsafe.ComputeHash(ptr, _testData.Length);
                Assert.Equal(BitConverter.ToUInt32(_testResults[typeof(DJBHash32)], 0), r1);

                //Farmhash32
                uint r2 = FarmHash32Unsafe.ComputeHash(ptr, _testData.Length);
                Assert.Equal(BitConverter.ToUInt32(_testResults[typeof(FarmHash32)], 0), r2);

                //Farmhash64
                ulong r3 = FarmHash64Unsafe.ComputeHash(ptr, _testData.Length);
                Assert.Equal(BitConverter.ToUInt64(_testResults[typeof(FarmHash64)], 0), r3);

                //FNV1A32
                uint r4 = FNV1A32Unsafe.ComputeHash(ptr, _testData.Length);
                Assert.Equal(BitConverter.ToUInt32(_testResults[typeof(FNV1A32)], 0), r4);

                //MurmurHash32
                uint r5 = MurmurHash32Unsafe.ComputeHash(ptr, _testData.Length);
                Assert.Equal(BitConverter.ToUInt32(_testResults[typeof(MurmurHash32)], 0), r5);

                //MurmurHash128
                byte[] r6 = MurmurHash128Unsafe.ComputeHash(ptr, _testData.Length);
                Assert.True(_testResults[typeof(MurmurHash128)].SequenceEqual(r6));

                //SipHash64
                ulong r7 = SipHash64Unsafe.ComputeHash(ptr, _testData.Length);
                Assert.Equal(BitConverter.ToUInt64(_testResults[typeof(SipHash64)], 0), r7);

                //SuperFastHash32
                uint r8 = SuperFastHash32Unsafe.ComputeHash(ptr, _testData.Length);
                Assert.Equal(BitConverter.ToUInt32(_testResults[typeof(SuperFastHash32)], 0), r8);

                //xxHash32
                uint r9 = xxHash32Unsafe.ComputeHash(ptr, _testData.Length);
                Assert.Equal(BitConverter.ToUInt32(_testResults[typeof(xxHash32)], 0), r9);

                //xxHash64
                ulong r10 = xxHash64Unsafe.ComputeHash(ptr, _testData.Length);
                Assert.Equal(BitConverter.ToUInt64(_testResults[typeof(xxHash64)], 0), r10);
            }
        }
    }

    [Fact]
    public void CheckAllHaveCorrectName()
    {
        foreach (Type type in GetAllTypesOf<HashAlgorithm>())
        {
            Assert.True(type.Name.Contains("32") || type.Name.Contains("64") || type.Name.Contains("128"));
        }
    }
}