using Genbox.FastHash.AbslHash;

namespace Genbox.FastHash.Tests.Single;

public class AbslHashTests
{
    [Theory]
    [InlineData("VprUGNH+5NnNRaORxgH/ySrZFQFDL+4VAodhfBNinmn8cg==", 0x531858a40bfa7ea1UL, 0x669da02f8d009e0fUL, 0xd6bdb2c9ba5e55f2UL)]
    [InlineData("gc1xZaY+q0nPcUvOOnWnT3bqfmT/geth/f7Dm2e/DemMfk4=", 0x86689478a7a7e8faUL, 0xceb19bf2255445cdUL, 0xffd3e23d4115a8aeUL)]
    [InlineData("Mr35fIxqx1ukPAL0su1yFuzzAU3wABCLZ8+ZUFsXn47UmAph", 0x4ec948b8e7f27288UL, 0x0e746992d6d43a7cUL, 0x2c3218ef486127deUL)]
    [InlineData("A9G8pw2+m7+rDtWYAdbl8tb2fT7FFo4hLi2vAsa5Y8mKH3CX3g==", 0x0ce46c7213c10032UL, 0x41ed623b9dcc5fdeUL, 0x554fa7f3a262b886UL)]
    [InlineData("DFaJGishGwEHDdj9ixbCoaTjz9KS0phLNWHVVdFsM93CvPft3hM=", 0xf63e96ee6f32a8b6UL, 0x187a5a30d7c72edcUL, 0x06304cbf82e312d3UL)]
    [InlineData("7+Ugx+Kr3aRNgYgcUxru62YkTDt5Hqis+2po81hGBkcrJg4N0uuy", 0x01cfe85e65fc5225UL, 0x949ae2a9c1eb925aUL, 0x490b3fb5af80622cUL)]
    [InlineData("H2w6O8BUKqu6Tvj2xxaecxEI2wRgIgqnTTG1WwOgDSINR13Nm4d4Vg==", 0x45c474f1cee1d2e8UL, 0x7e9c76a7b7c35e68UL, 0x7398a90b8cc59c5dUL)]
    [InlineData("1XBMnIbqD5jy65xTDaf6WtiwtdtQwv1dCVoqpeKj+7cTR1SaMWMyI04=", 0x6e024e14015f329cUL, 0x4f96bf15b8309ff6UL, 0x65fb3168b98030abUL)]
    [InlineData("znZbdXG2TSFrKHEuJc83gPncYpzXGbAebUpP0XxzH0rpe8BaMQ17nDbt", 0x760c40502103ae1cUL, 0x26c0c1fde233732eUL, 0xd4564363c53617bbUL)]
    [InlineData("ylu8Atu13j1StlcC1MRMJJXIl7USgDDS22HgVv0WQ8hx/8pNtaiKB17hCQ==", 0x17fd05c3c560c320UL, 0x0b0453f72aa151615UL, 0x0545c26351925fe7UL)]
    public void AbseilLowLevelGoldenVectors(string base64Data, ulong seed, ulong scalarExpected, ulong simdExpected)
    {
        byte[] data = Convert.FromBase64String(base64Data);
        ulong expected = AbslHash64.IsSimdSupported ? simdExpected : scalarExpected;

        Assert.True(data.Length > 32);
        Assert.Equal(expected, AbslHash64.ComputeHash(data, seed));

        unsafe
        {
            fixed (byte* ptr = data)
                Assert.Equal(expected, AbslHash64Unsafe.ComputeHash(ptr, data.Length, seed));
        }
    }

    [Fact]
    public unsafe void UnsafeMatchesManaged()
    {
        byte[] data = Enumerable.Range(0, 5000).Select(static x => unchecked((byte)x)).ToArray();
        ulong[] seeds = [0, 1, 0x531858a40bfa7ea1UL, ulong.MaxValue];

        fixed (byte* ptr = data)
        {
            foreach (ulong seed in seeds)
            {
                for (int len = 0; len <= data.Length; len++)
                    Assert.Equal(AbslHash64.ComputeHash(data.AsSpan(0, len), seed), AbslHash64Unsafe.ComputeHash(ptr, len, seed));
            }
        }
    }

    [Fact]
    public void AbseilStringCases()
    {
        byte[] empty = [];
        byte[] small = "foo"u8.ToArray();
        byte[] dup = "foofoo"u8.ToArray();
        byte[] large = Enumerable.Repeat((byte)'x', 2048).ToArray();
        byte[] huge = Enumerable.Repeat((byte)'a', 5000).ToArray();

        Assert.Equal(AbslHash64.ComputeHash(empty), AbslHash64.ComputeHash([]));
        Assert.Equal(AbslHash64.ComputeHash(small), AbslHash64.ComputeHash("foo"u8));
        Assert.NotEqual(AbslHash64.ComputeHash(small), AbslHash64.ComputeHash(dup));
        Assert.NotEqual(AbslHash64.ComputeHash(large), AbslHash64.ComputeHash(huge));
    }
}