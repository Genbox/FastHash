using System.Text;
using Genbox.FastHash.XxHash;

namespace Genbox.FastHash.Examples;

internal static class Program
{
    public static void Main()
    {
        byte[] data = Encoding.UTF8.GetBytes("Hello, FastHash!");
        Console.WriteLine($"XXH64: {XxHash64.ComputeHash(data):x16}");
        Console.WriteLine($"Seeded XXH64: {XxHash64.ComputeHash(data, 42):x16}");
        Console.WriteLine($"Integer index: {XxHash64.ComputeIndex(42):x16}");
    }
}