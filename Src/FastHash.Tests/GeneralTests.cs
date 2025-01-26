using System.Reflection;
using System.Security.Cryptography;
using Genbox.FastHash.DjbHash;

namespace Genbox.FastHash.Tests;

public class GeneralTests
{
    [Theory, MemberData(nameof(GetAllTypesOf))]
    public void CheckAllHaveCorrectName(Type type)
    {
        Assert.True(type.Name.EndsWith("32", StringComparison.Ordinal) ||
                    type.Name.EndsWith("32Unsafe", StringComparison.Ordinal) ||
                    type.Name.EndsWith("64", StringComparison.Ordinal) ||
                    type.Name.EndsWith("64Unsafe", StringComparison.Ordinal) ||
                    type.Name.EndsWith("128", StringComparison.Ordinal) ||
                    type.Name.EndsWith("128Unsafe", StringComparison.Ordinal));
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