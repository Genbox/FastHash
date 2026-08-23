using System.Reflection;
using Genbox.FastHash.DjbHash;

namespace Genbox.FastHash.Tests;

public class GeneralTests
{
    [Fact]
    public void UInt128HasValueEquality()
    {
        UInt128 value = new UInt128(1, 2);
        UInt128 equalValue = new UInt128(1, 2);
        UInt128 differentLow = new UInt128(3, 2);
        UInt128 differentHigh = new UInt128(1, 4);

        Assert.True(value.Equals(equalValue));
        Assert.True(value.Equals((object)equalValue));
        Assert.False(value.Equals((object?)null));
        Assert.Equal(value.GetHashCode(), equalValue.GetHashCode());
        Assert.True(value == equalValue);
        Assert.True(value != differentLow);
        Assert.True(value != differentHigh);
        const BindingFlags fieldFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        Assert.True(typeof(UInt128).GetField(nameof(UInt128.Low), fieldFlags)!.IsInitOnly);
        Assert.True(typeof(UInt128).GetField(nameof(UInt128.High), fieldFlags)!.IsInitOnly);
    }

    [Theory, MemberData(nameof(GetAllTypesOf))]
    public void CheckAllHaveCorrectName(Type type)
    {
        Assert.True(type.Name.EndsWith("32", StringComparison.Ordinal) ||
                    type.Name.EndsWith("32Unsafe", StringComparison.Ordinal) ||
                    type.Name.EndsWith("64", StringComparison.Ordinal) ||
                    type.Name.EndsWith("64Unsafe", StringComparison.Ordinal) ||
                    type.Name.EndsWith("128", StringComparison.Ordinal) ||
                    type.Name.EndsWith("128Unsafe", StringComparison.Ordinal) ||
                    type.Name.EndsWith("256", StringComparison.Ordinal) ||
                    type.Name.EndsWith("256Unsafe", StringComparison.Ordinal));
    }

    public static TheoryData<Type> GetAllTypesOf()
    {
        TheoryData<Type> data = new TheoryData<Type>();

        Assembly assembly = typeof(Djb2Hash32).GetTypeInfo().Assembly;

        foreach (Type type in assembly.GetTypes())
        {
            if (type.Name.Contains("Shared", StringComparison.Ordinal) || type.Name.Contains("Constants", StringComparison.Ordinal))
                continue;

            if (type.Name == "MixFunctions")
                continue;

            if (type.IsPublic && type.IsAbstract && type.IsSealed)
                data.Add(type);
        }

        return data;
    }
}