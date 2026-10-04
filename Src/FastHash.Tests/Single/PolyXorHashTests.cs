// Ports upstream tests from orlp/polyxor 0.1.0 (3123eb6), with streaming coverage.
// Copyright (c) 2026 Orson Peters. Zlib license: FastHash/PolyXorHash/LICENSE.txt.
using System.Buffers.Binary;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using Genbox.FastHash.PolyXorHash;
using ArmAes = System.Runtime.Intrinsics.Arm.Aes;
using Aes = System.Security.Cryptography.Aes;

namespace Genbox.FastHash.Tests.Single;

public class PolyXorHashTests
{
    private const ulong Tweak = 0x0123456789abcdef;
    private static readonly UInt128 Key = new UInt128(0x0123456789abcdef, 0xfedcba9876543210);

    [Fact]
    public unsafe void ComputeHash_EntropyParameters_MatchesUpstreamVectors()
    {
        byte[] entropy = new byte[PolyXorHashParams.EntropyNeeded];
        for (int i = 0; i < entropy.Length; i++)
            entropy[i] = unchecked((byte)((i * 7) + 3));
        PolyXorHashParams parameters = PolyXorHashParams.FromEntropy(entropy);
        (byte[] Message, string Raw, string Avalanche, string Tweaked)[] cases =
        [
            ([], "71674dc8618a59aea5e747e3e8e65cb1", "18e17e168ebb1e2546333eedd3e8d4cb", "463831961cbb78bc09b4e223ae762653"),
            ("abc"u8.ToArray(), "768ec4cb13ed00df813dd5a56ee735c7", "9d8c4cf7c0441c2ea888298e36e23a2b", "ad828d5044a86b58b3099cff9bd00846"),
            (Pattern(5000), "f7d15316f72aa4bafac50b723c904dba", "dc08376d948de1d510b80d819ca6f4de", "7b52a8346b7c0a938f0333c3b57c85a3")
        ];

        foreach ((byte[] Message, string Raw, string Avalanche, string Tweaked) c in cases)
        {
            UInt128 raw = PolyXorReference.Raw(c.Message, parameters);
            Assert.Equal(Parse(c.Raw), raw);
            Assert.Equal(Parse(c.Avalanche), PolyXorReference.Avalanche(raw, parameters));
            Assert.Equal(Parse(c.Tweaked), PolyXorReference.Avalanche(raw, parameters, Tweak));
            if (!PolyXorHash128.IsSupported)
                continue;

            PolyXorHasher hasher = parameters.CreateHasher();
            hasher.Update(c.Message);
            Assert.Equal(Parse(c.Raw), hasher.FinalizeRaw());
            Assert.Equal(Parse(c.Avalanche), hasher.FinalizeAvalanche());
            Assert.Equal(Parse(c.Tweaked), hasher.FinalizeAvalanche(Tweak));
            Assert.Equal(Parse(c.Raw), PolyXorHash128.ComputeHashRaw(c.Message, parameters));
            Assert.Equal(Parse(c.Avalanche), PolyXorHash128.ComputeHash(c.Message, parameters));
            fixed (byte* ptr = c.Message)
                Assert.Equal(Parse(c.Tweaked), PolyXorHash128Unsafe.ComputeHash(ptr, c.Message.Length, parameters, Tweak));
        }
    }

    [Fact]
    public unsafe void ComputeHash_AesKey_MatchesUpstreamVectors()
    {
        PolyXorHashParams parameters = new PolyXorHashParams(Key);
        (byte[] Message, string Raw, string Mac)[] cases =
        [
            ([], "848d40b38af689d1bf33645504916ccf", "1ddadfc23088181c7772364ac0693d8c"),
            ("abc"u8.ToArray(), "c50ca7f96a0eeafd827bc6e264cda461", "9f561bc3356a93411738fb36838c5dda"),
            (Pattern(5000), "569abc56a85b9928d5616e900ebd9eb9", "05edacb22bf0eccb88d0344b97f2b485")
        ];

        foreach ((byte[] Message, string Raw, string Mac) c in cases)
        {
            UInt128 raw = PolyXorReference.Raw(c.Message, parameters);
            Assert.Equal(Parse(c.Raw), raw);
            Assert.Equal(Parse(c.Mac), ReferenceMac(raw, Key, new UInt128(0, 42)));
            if (!PolyXorHash128.IsSupported)
                continue;
            PolyXorHasher hasher = parameters.CreateHasher();
            hasher.Update(c.Message);
            Assert.Equal(Parse(c.Raw), hasher.FinalizeRaw());
            Assert.Equal(Parse(c.Raw), PolyXorHash128.ComputeHashRaw(c.Message, parameters));
            Assert.Equal(Parse(c.Raw), PolyXorHash128.ComputeHashRaw(c.Message, Key));
            UInt128 avalanche = PolyXorReference.Avalanche(raw, parameters);
            UInt128 tweaked = PolyXorReference.Avalanche(raw, parameters, Tweak);
            Assert.Equal(avalanche, PolyXorHash128.ComputeHash(c.Message, Key));
            Assert.Equal(tweaked, PolyXorHash128.ComputeHash(c.Message, Key, Tweak));

            fixed (byte* ptr = c.Message)
            {
                Assert.Equal(avalanche, PolyXorHash128Unsafe.ComputeHash(ptr, c.Message.Length, Key));
                Assert.Equal(tweaked, PolyXorHash128Unsafe.ComputeHash(ptr, c.Message.Length, Key, Tweak));
            }

            if (System.Runtime.Intrinsics.X86.Aes.IsSupported || ArmAes.IsSupported)
            {
                Assert.Equal(Parse(c.Mac), hasher.FinalizeMac(new UInt128(0, 42)));
                Assert.Equal(Parse(c.Mac), PolyXorHash128.ComputeMac(c.Message, parameters, new UInt128(0, 42)));
                Assert.Equal(Parse(c.Mac), PolyXorHash128.ComputeMac(c.Message, Key, new UInt128(0, 42)));
            }
        }
    }

    [Fact]
    public void ComputeHash_DefaultKey_MatchesCatalogVector()
    {
        PolyXorHashParams parameters = new PolyXorHashParams(default);
        UInt128 raw = PolyXorReference.Raw("This is a test!!"u8, parameters);
        Assert.Equal(Parse("398e2557b5b2b0f143986d0679d26651"), PolyXorReference.Avalanche(raw, parameters));
    }

    [Fact]
    public void ComputeHash_UnsupportedHardware_RejectsHashAndIndexOperations()
    {
        if (PolyXorHash128.IsSupported)
            return;
        PolyXorHashParams parameters = PolyXorHashParams.FromEntropy(new byte[PolyXorHashParams.EntropyNeeded]);
        Assert.Throws<PlatformNotSupportedException>(() => PolyXorHash128.ComputeHash("abc"u8, parameters));
        Assert.Throws<PlatformNotSupportedException>(() => PolyXorHash128.ComputeIndex(42, parameters));
        Assert.Throws<PlatformNotSupportedException>(() => parameters.CreateHasher());
    }

    [Fact]
    public void ComputeIndex_KeysAndTweaks_MatchesReference()
    {
        if (!PolyXorHash128.IsSupported)
            return;

        UInt128[] keys = [default, Key, new UInt128(ulong.MaxValue, ulong.MaxValue)];
        PolyXorHashParams[] parameters =
        [
            new PolyXorHashParams(keys[0]),
            new PolyXorHashParams(keys[1]),
            new PolyXorHashParams(keys[2]),
            PolyXorHashParams.FromEntropy(PolyXorReference.RandomBytes(PolyXorHashParams.EntropyNeeded, 2)),
            PolyXorHashParams.FromEntropy(PolyXorReference.RandomBytes(PolyXorHashParams.EntropyNeeded, 7)),
            PolyXorHashParams.FromEntropy(new byte[PolyXorHashParams.EntropyNeeded])
        ];
        List<ulong> inputs = [0, ulong.MaxValue, 0x5555555555555555, 0xaaaaaaaaaaaaaaaa, 0x0123456789abcdef, 0xfedcba9876543210];
        for (int bit = 0; bit < 64; bit++)
            inputs.Add(1UL << bit);
        byte[] random = PolyXorReference.RandomBytes(64 * 8, 5);
        for (int i = 0; i < 64; i++)
            inputs.Add(BinaryPrimitives.ReadUInt64LittleEndian(random.AsSpan(i * 8)));

        byte[] message = new byte[8];

        for (int i = 0; i < parameters.Length; i++)
        {
            foreach (ulong input in inputs)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(message, input);
                UInt128 raw = PolyXorReference.Raw(message, parameters[i]);
                Assert.Equal(raw, PolyXorHashShared.ToUInt128(PolyXorHashShared.HashIndexRaw(input, parameters[i])));

                foreach (ulong tweak in new[] { 0UL, Tweak, ulong.MaxValue })
                {
                    UInt128 expected = PolyXorReference.Avalanche(raw, parameters[i], tweak);
                    Assert.Equal(expected, PolyXorHash128.ComputeIndex(input, parameters[i], tweak));
                    if (i < keys.Length)
                        Assert.Equal(expected, PolyXorHash128.ComputeIndex(input, keys[i], tweak));
                }
            }
        }
    }

    [Fact]
    public void PolyXorHashParams_Keys_MatchesAesProvider()
    {
        foreach (UInt128 key in new[] { default(UInt128), Key, new UInt128(ulong.MaxValue, ulong.MaxValue) })
        {
            byte[] entropy = new byte[PolyXorHashParams.EntropyNeeded];

            for (int i = 0; i < entropy.Length / 16; i++)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(entropy.AsSpan(i * 16), (ulong)i);
                BinaryPrimitives.WriteUInt64LittleEndian(entropy.AsSpan((i * 16) + 8), 1UL << 63);
            }

            using Aes aes = Aes.Create();
            aes.Key = KeyBytes(key);
            aes.EncryptEcb(entropy, entropy, PaddingMode.None);
            PolyXorHashParams expected = PolyXorHashParams.FromEntropy(entropy);
            PolyXorHashParams actual = new PolyXorHashParams(key);
            Assert.Equal(expected.BlockKey, actual.BlockKey);
            Assert.Equal(expected.PolyZ, actual.PolyZ);
            Assert.Equal(expected.PolyU, actual.PolyU);
            Assert.Equal(expected.PolyY, actual.PolyY);
            Assert.Equal(expected.AvalancheMul, actual.AvalancheMul);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public unsafe void ComputeHash_ShortSpans_MatchesReferenceAndStreaming(int length)
    {
        if (!PolyXorHash128.IsSupported)
            return;

        PolyXorHashParams[] parameters =
        [
            new PolyXorHashParams(default),
            new PolyXorHashParams(Key),
            new PolyXorHashParams(new UInt128(ulong.MaxValue, ulong.MaxValue)),
            PolyXorHashParams.FromEntropy(PolyXorReference.RandomBytes(PolyXorHashParams.EntropyNeeded, 11)),
            PolyXorHashParams.FromEntropy(new byte[PolyXorHashParams.EntropyNeeded])
        ];
        List<byte[]> messages = [new byte[length], Enumerable.Repeat((byte)0xff, length).ToArray(), PolyXorReference.RandomBytes(length, 5)];

        for (int bit = 0; bit < length * 8; bit++)
        {
            byte[] message = new byte[length];
            message[bit / 8] = (byte)(1 << (bit % 8));
            messages.Add(message);
        }

        foreach (PolyXorHashParams p in parameters)
        {
            PolyXorHasher hasher = p.CreateHasher();
            hasher.Update(Pattern(129));
            hasher.Reset(); // Short finalization must ignore the old bytes beyond Count.

            foreach (byte[] message in messages)
            {
                UInt128 raw = PolyXorReference.Raw(message, p);
                UInt128 expectedRaw = raw;
                UInt128 expected = PolyXorReference.Avalanche(raw, p);
                UInt128 tweaked = PolyXorReference.Avalanche(raw, p, Tweak);

                foreach (int offset in new[] { 0, 1, 3, 7 })
                {
                    // Nonzero surrounding bytes expose accidental reads outside a short span.
                    byte[] buffer = Enumerable.Repeat((byte)0xa5, offset + length + 8).ToArray();
                    message.CopyTo(buffer, offset);
                    ReadOnlySpan<byte> data = buffer.AsSpan(offset, length);
                    Assert.Equal(expectedRaw, PolyXorHash128.ComputeHashRaw(data, p));
                    Assert.Equal(expected, PolyXorHash128.ComputeHash(data, p));
                    Assert.Equal(tweaked, PolyXorHash128.ComputeHash(data, p, Tweak));
                    fixed (byte* ptr = buffer)
                        Assert.Equal(tweaked, PolyXorHash128Unsafe.ComputeHash(ptr + offset, length, p, Tweak));
                }

                hasher.Reset();
                int split = length / 2;
                hasher.Update(message.AsSpan(0, split));
                UInt128 prefix = PolyXorReference.Raw(message.AsSpan(0, split), p);
                Assert.Equal(prefix, hasher.FinalizeRaw());
                Assert.Equal(prefix, hasher.FinalizeRaw());
                hasher.Update(message.AsSpan(split));
                Assert.Equal(expectedRaw, hasher.FinalizeRaw());
                Assert.Equal(expected, hasher.FinalizeAvalanche());
                Assert.Equal(tweaked, hasher.FinalizeAvalanche(Tweak));
            }
        }

        if (System.Runtime.Intrinsics.X86.Aes.IsSupported || ArmAes.IsSupported)
        {
            byte[] message = PolyXorReference.RandomBytes(length, 7);
            UInt128 raw = PolyXorReference.Raw(message, parameters[1]);
            UInt128 nonce = new UInt128(0, 42);
            UInt128 expectedMac = ReferenceMac(raw, Key, nonce);
            Assert.Equal(expectedMac, PolyXorHash128.ComputeMac(message, parameters[1], nonce));
            PolyXorHasher hasher = parameters[1].CreateHasher();
            hasher.Update(message);
            Assert.Equal(expectedMac, hasher.FinalizeMac(nonce));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void ComputeHash_PaddedTailBoundaries_MatchesReference(int wholeChunks)
    {
        if (!PolyXorHash128.IsSupported)
            return;

        PolyXorHashParams parameters = PolyXorHashParams.FromEntropy(PolyXorReference.RandomBytes(PolyXorHashParams.EntropyNeeded, 13));
        byte[] buffer = PolyXorReference.RandomBytes(1 + ((wholeChunks + 1) * 4096), 17);
        SortedSet<int> tails = [0, 8, 9, 16, 32, 64];

        for (int boundary = 128; boundary <= 4096; boundary += 128)
        {
            tails.Add(boundary - 1);

            if (boundary < 4096)
            {
                tails.Add(boundary);
                tails.Add(boundary + 1);
            }
        }

        foreach (int tail in tails)
        {
            ReadOnlySpan<byte> data = buffer.AsSpan(1, (wholeChunks * 4096) + tail);
            UInt128 raw = PolyXorReference.Raw(data, parameters);
            Assert.Equal(raw, PolyXorHash128.ComputeHashRaw(data, parameters));
            Assert.Equal(PolyXorReference.Avalanche(raw, parameters), PolyXorHash128.ComputeHash(data, parameters));
        }
    }

    [Fact]
    public void Multiply_PolyvalVector_MatchesRfc8452()
    {
        UInt128 h = Parse("7b754bba26f8311d7642925847936225");
        UInt128 x1 = Parse("62a2012dbb621740b6df838c66954f4f");
        UInt128 x2 = Parse("62f3c9d3205fe4bb06d02127dd4da2d1");
        UInt128 expected = Parse("7eb7e5f56c86b7e5fa1961847bb4a3f7");
        Assert.Equal(expected, PolyXorReference.Multiply(PolyXorReference.Multiply(x1, h) ^ x2, h));
        if (PolyXorHash128.IsSupported)
            Assert.Equal(expected, PolyXorHashShared.ToUInt128(PolyXorHashShared.Multiply(PolyXorHashShared.Multiply(Vector(x1), Vector(h)) ^ Vector(x2), Vector(h))));
    }

    [Fact]
    public void Multiply_DeterministicOperands_MatchesReference()
    {
        if (!PolyXorHash128.IsSupported)
            return;
        byte[] bytes = PolyXorReference.RandomBytes(16 * 64, 1);

        for (int i = 0; i < 64; i++)
        {
            for (int j = 0; j < 64; j++)
            {
                UInt128 a = PolyXorReference.Load(bytes.AsSpan(i * 16));
                UInt128 b = PolyXorReference.Load(bytes.AsSpan(j * 16));
                Assert.Equal(PolyXorReference.Multiply(a, b), PolyXorHashShared.ToUInt128(PolyXorHashShared.Multiply(Vector(a), Vector(b))));
            }
        }
    }

    [Theory]
    [InlineData(128)]
    [InlineData(256)]
    [InlineData(512)]
    public void HashBlocks_UnalignedInputs_MatchesReference(int width)
    {
        Assert.SkipWhen(!BackendSupported(width), $"The {width}-bit PolyXOR backend is unavailable on this CPU.");
        PolyXorHashParams parameters = PolyXorHashParams.FromEntropy(PolyXorReference.RandomBytes(PolyXorHashParams.EntropyNeeded, 2));
        byte[] buffer = PolyXorReference.RandomBytes(1 + (3 * 4096) + 128, 3);

        for (int blocks = 0; blocks <= (buffer.Length - 1) / 128; blocks++)
        {
            ReadOnlySpan<byte> data = buffer.AsSpan(1, blocks * 128);
            Vector128<ulong> accum = parameters.PolyZ;
            UInt128 initial = ((UInt128)accum.GetElement(1) << 64) | accum.GetElement(0);
            UInt128 expected = PolyXorReference.HashBlocks(data, parameters, initial);
            if (width == 512)
                PolyXorHashX86Shared.Hash512(data, parameters, ref accum);
            else if (width == 256)
                PolyXorHashX86Shared.Hash256(data, parameters, ref accum);
            else
                PolyXorHashShared.Hash128(data, parameters, ref accum);
            Assert.Equal(expected, PolyXorHashShared.ToUInt128(accum));
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(127)]
    [InlineData(128)]
    [InlineData(129)]
    [InlineData(4095)]
    [InlineData(4096)]
    [InlineData(4097)]
    [InlineData(4098)]
    [InlineData(4099)]
    [InlineData(4100)]
    [InlineData(4101)]
    [InlineData(4102)]
    [InlineData(4103)]
    [InlineData(4104)]
    [InlineData(8200)]
    [InlineData(12417)]
    public unsafe void Update_BoundaryChunkSizes_MatchesReferenceAndOneShot(int length)
    {
        if (!PolyXorHash128.IsSupported)
            return;
        PolyXorHashParams parameters = new PolyXorHashParams(Key);
        byte[] data = PolyXorReference.RandomBytes(length + 1, 4);
        ReadOnlySpan<byte> message = data.AsSpan(1);
        UInt128 raw = PolyXorReference.Raw(message, parameters);
        UInt128 expected = PolyXorReference.Avalanche(raw, parameters, Tweak);
        Assert.Equal(expected, PolyXorHash128.ComputeHash(message, parameters, Tweak));
        fixed (byte* ptr = data)
            Assert.Equal(expected, PolyXorHash128Unsafe.ComputeHash(ptr + 1, length, parameters, Tweak));

        foreach (int chunk in new[] { 1, 127, 128, 129, 4095, 4096, 4097 })
        {
            PolyXorHasher hasher = parameters.CreateHasher();

            for (int i = 0; i < message.Length; i += chunk)
            {
                hasher.Update(message.Slice(i, Math.Min(chunk, message.Length - i)));

                // Finalization must not commit zero padding or a polynomial step.
                Assert.Equal(hasher.FinalizeRaw(), hasher.FinalizeRaw());
                hasher.Update([]);
            }

            Assert.Equal((ulong)length, hasher.Count);
            Assert.Equal(raw, hasher.FinalizeRaw());
            Assert.Equal(expected, hasher.FinalizeAvalanche(Tweak));
            if (System.Runtime.Intrinsics.X86.Aes.IsSupported || ArmAes.IsSupported)
                Assert.Equal(ReferenceMac(raw, Key, new UInt128(0, 42)), hasher.FinalizeMac(new UInt128(0, 42)));
            hasher.Reset();
            Assert.Equal(0UL, hasher.Count);
            Assert.Equal(PolyXorHash128.ComputeHashRaw([], parameters), hasher.FinalizeRaw());
            hasher.Update(message);
            Assert.Equal(expected, hasher.FinalizeAvalanche(Tweak));
        }
    }

    [Fact]
    public void Clone_FinalizedPrefix_AllowsIndependentContinuation()
    {
        if (!PolyXorHash128.IsSupported)
            return;
        PolyXorHashParams parameters = new PolyXorHashParams(Key);
        PolyXorHasher hasher = parameters.CreateHasher();
        byte[] prefix = Pattern(4200);
        hasher.Update(prefix);
        _ = hasher.FinalizeAvalanche();
        PolyXorHasher clone = hasher.Clone();
        hasher.Update("abc"u8);
        clone.Update("def"u8);
        Assert.Equal(PolyXorHash128.ComputeHash([.. prefix, .. "abc"u8], parameters), hasher.FinalizeAvalanche());
        Assert.Equal(PolyXorHash128.ComputeHash([.. prefix, .. "def"u8], parameters), clone.FinalizeAvalanche());
    }

    [Fact]
    public void FinalizeMac_NonceDomainsAndEntropyParameters_MatchesUpstream()
    {
        Assert.Throws<ArgumentException>(() => PolyXorHashParams.FromEntropy(new byte[PolyXorHashParams.EntropyNeeded - 1]));
        if (!PolyXorHash128.IsSupported)
            return;
        PolyXorHashParams entropy = PolyXorHashParams.FromEntropy(new byte[PolyXorHashParams.EntropyNeeded]);
        Assert.Equal(Vector128.Create(1UL, 0UL), entropy.AvalancheMul);
        Assert.Throws<InvalidOperationException>(() => entropy.CreateHasher().FinalizeMac(default));
        if (!System.Runtime.Intrinsics.X86.Aes.IsSupported && !ArmAes.IsSupported)
            return;
        PolyXorHashParams parameters = new PolyXorHashParams(Key);
        UInt128 nonce = new UInt128(0, 42);
        UInt128 tag = PolyXorHash128.ComputeMac("abc"u8, parameters, nonce);
        Assert.Equal(tag, PolyXorHash128.ComputeMac("abc"u8, parameters, new UInt128(3UL << 62, 42)));
        Assert.NotEqual(tag, PolyXorHash128.ComputeMac("abc"u8, parameters, new UInt128(0, 43)));
    }

    private static bool BackendSupported(int width) => width switch
    {
        128 => PolyXorHash128.IsSupported,
        256 => Avx2.IsSupported && Pclmulqdq.V256.IsSupported,
        512 => Avx512F.IsSupported && Avx512F.VL.IsSupported && Pclmulqdq.V512.IsSupported,
        _ => false
    };

    private static UInt128 Parse(string hex) => new UInt128(Convert.ToUInt64(hex[..16], 16), Convert.ToUInt64(hex[16..], 16));
    private static Vector128<ulong> Vector(UInt128 value) => Vector128.Create(unchecked((ulong)value), (ulong)(value >> 64));
    private static byte[] Pattern(int length) => Enumerable.Range(0, length).Select(static i => unchecked((byte)i)).ToArray();

    private static byte[] KeyBytes(UInt128 key)
    {
        byte[] bytes = new byte[16];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, key.Low);
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(8), key.High);
        return bytes;
    }

    private static UInt128 ReferenceMac(UInt128 raw, UInt128 key, UInt128 nonce)
    {
        UInt128 n = nonce, mask = UInt128.MaxValue >> 2;
        byte[] blocks = new byte[32];
        UInt128 a = n & mask, b = ((raw ^ n) & mask) | ((UInt128)1 << 126);
        KeyBytes(a).CopyTo(blocks, 0);
        KeyBytes(b).CopyTo(blocks, 16);
        using Aes aes = Aes.Create();
        aes.Key = KeyBytes(key);
        byte[] encrypted = aes.EncryptEcb(blocks, PaddingMode.None);
        return PolyXorReference.Load(encrypted) ^ PolyXorReference.Load(encrypted.AsSpan(16));
    }
}