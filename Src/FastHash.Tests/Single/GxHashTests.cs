using System.Runtime.InteropServices;
using Genbox.FastHash.GxHash;

namespace Genbox.FastHash.Tests.Single;

public class GxHashTests
{
    [Fact]
    public void ValuesTest()
    {
        Assert.Equal(456576800u, GxHash32.ComputeHash(Array.Empty<byte>().AsSpan(), 0));
        Assert.Equal(978957914u, GxHash32.ComputeHash(new byte[1].AsSpan(), 0));
        Assert.Equal(3325885698u, GxHash32.ComputeHash(new byte[1000].AsSpan(), 0));
        Assert.Equal(1827274036u, GxHash32.ComputeHash(MemoryMarshal.AsBytes("hello world".AsSpan()), 123));
    }

    [Fact]
    public void SanityChecks()
    {
        HashSet<ulong> hashes = new HashSet<ulong>();

        // Check that zero filled inputs are hashes differently depending on their size
        byte[] bytes = new byte[1000];
        for (int i = 0; i < bytes.Length; i++)
        {
            ReadOnlySpan<byte> slice = bytes.AsSpan().Slice(0, i);
            ulong hash = GxHash64.ComputeHash(slice, 42);
            Assert.NotEqual(0UL, hash);
            Assert.True(hashes.Add(hash));
        }

        // Check that zero padding affects output hash
        hashes.Clear();
        bytes[0] = 123;
        for (int i = 0; i < bytes.Length; i++)
        {
            ReadOnlySpan<byte> slice = bytes.AsSpan().Slice(0, i);
            ulong hash = GxHash64.ComputeHash(slice, 42);
            Assert.NotEqual(0UL, hash);
            Assert.True(hashes.Add(hash));
        }
    }

    [Fact]
    public void AllBytesAreRead()
    {
        for (int s = 0; s < 1200; s++)
        {
            byte[] bytes = new byte[s];
            uint hash = GxHash32.ComputeHash(bytes, 42);

            for (int i = 0; i < s; i++)
            {
                byte swap = bytes[i];
                bytes[i] = 82;
                uint newHash = GxHash32.ComputeHash(bytes, 42);
                bytes[i] = swap;

                Assert.NotEqual(hash, newHash);
            }
        }
    }

    [Theory]
    [InlineData(1, 0, 1)]
    [InlineData(1, 0, 16)]
    [InlineData(1, 0, 32)]
    [InlineData(16, 0, 16)]
    [InlineData(16, 0, 32)]
    [InlineData(16, 16, 32)]
    [InlineData(16, 32, 48)]
    [InlineData(32, 0, 32)]
    [InlineData(32, 0, 64)]
    [InlineData(32, 32, 64)]
    [InlineData(32, 64, 96)]
    public void BytesOrderMatters(int swapSize, int swapPositionA, int swapPositionB)
    {
        Random rnd = new Random(123);
        byte[] bytes = new byte[255];
        rnd.NextBytes(bytes);

        uint hash = GxHash32.ComputeHash(bytes, 0);

        SwapBytes(bytes, swapPositionA, swapPositionB, swapSize);

        uint hashAfterSwap = GxHash32.ComputeHash(bytes, 0);

        Assert.NotEqual(hash, hashAfterSwap);
    }

    private static void SwapBytes(Span<byte> span, int pos1, int pos2, int n)
    {
        // Check if the input parameters are valid
        if (pos1 < 0 || pos2 < 0 || n < 0)
            throw new ArgumentOutOfRangeException("Positions and length must be non-negative.");

        if (pos1 + n > span.Length || pos2 + n > span.Length)
            throw new ArgumentOutOfRangeException("Positions and length must be within the span's length.");

        // Perform the swap
        Span<byte> temp = stackalloc byte[n];
        span.Slice(pos1, n).CopyTo(temp);
        span.Slice(pos2, n).CopyTo(span.Slice(pos1, n));
        temp.CopyTo(span.Slice(pos2, n));
    }
}