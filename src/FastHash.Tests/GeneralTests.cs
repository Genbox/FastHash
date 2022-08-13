using System.Reflection;
using System.Security.Cryptography;
using Genbox.FastHash.DjbHash;
using Xunit;

namespace Genbox.FastHash.Tests;

public class GeneralTests
{
    [Fact]
    public void CheckAllHaveCorrectName()
    {
        foreach (Type type in GetAllTypesOf<HashAlgorithm>())
            Assert.True(type.Name.Contains("32") || type.Name.Contains("64") || type.Name.Contains("128"));
    }

    private IEnumerable<Type> GetAllTypesOf<T>()
    {
        Assembly assembly = typeof(Djb2Hash32).GetTypeInfo().Assembly;

        foreach (Type type in assembly.GetTypes())
        {
            if (type.GetInterfaces().Any(x => x == typeof(T)))
                yield return type;

            if (type.GetTypeInfo().BaseType == typeof(T))
                yield return type;
        }
    }
}