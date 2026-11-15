#if NET8_0_OR_GREATER

// C# port of ogxd/gxhash 3.5.0 (6438a7b). Copyright (c) 2023 Olivier Giniaux.
// Distributed under the MIT license; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using ArmAes = System.Runtime.Intrinsics.Arm.Aes;
using X86Aes = System.Runtime.Intrinsics.X86.Aes;

namespace Genbox.FastHash.GxHash;

internal static class Gx2HashShared
{
    private const string UnsupportedMessage = "Gx2Hash requires AES intrinsics and AdvSimd on ARM.";
    private const int VECTOR_SIZE = 16;
    private const int UNROLL_FACTOR = 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> Compute(ReadOnlySpan<byte> bytes, long seed = 0) => Finalize(AesEncrypt(Compress(bytes), CreateSeed(seed)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> Compute(Vector128<byte> input, long seed = 0) => Finalize(AesEncrypt(input, CreateSeed(seed)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> CreateSeed(long seed) => Vector128.Create(unchecked((ulong)seed), unchecked((ulong)seed)).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Finalize(Vector128<byte> input)
    {
        input = AesEncrypt(input, Key0());
        input = AesEncrypt(input, Key1());
        return AesEncryptLast(input, Key2());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Compress(ReadOnlySpan<byte> bytes)
    {
        int len = bytes.Length;

        if (len == 0)
            return Vector128<byte>.Zero;

        ref byte ptr = ref MemoryMarshal.GetReference(bytes);

        if (len <= VECTOR_SIZE)
        {
            return Vector128.Add(ReadPartial128(ref ptr, len), Vector128.Create((byte)len));
        }

        Vector128<byte> hashVector;
        int offset;

        int extraBytesCount = len % VECTOR_SIZE;

        if (extraBytesCount == 0)
        {
            hashVector = Unsafe.ReadUnaligned<Vector128<byte>>(ref ptr);
            offset = VECTOR_SIZE;
        }
        else
        {
            // The complete first vector is inside this input (len > 16).
            // Mask it down to the leading partial block before processing full blocks.
            hashVector = GetMaskedVector(ref ptr, extraBytesCount);
            offset = extraBytesCount;
        }

        Vector128<byte> tail = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref ptr, offset));
        offset += VECTOR_SIZE;

        if (len > VECTOR_SIZE * 2)
        {
            // Fast path when input length > 32 and <= 48
            Vector128<byte> next = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref ptr, offset));
            offset += VECTOR_SIZE;
            tail = AesEncrypt(tail, next);

            if (len > VECTOR_SIZE * 3)
            {
                // Fast path when input length > 48 and <= 64
                next = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref ptr, offset));
                offset += VECTOR_SIZE;
                tail = AesEncrypt(tail, next);

                if (len > VECTOR_SIZE * 4)
                    hashVector = CompressMany(ref ptr, offset, len, hashVector);
            }
        }

        return AesEncryptLast(hashVector, AesEncrypt(AesEncrypt(tail, Key0()), Key1()));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> CompressMany(ref byte start, int offset, int end, Vector128<byte> hashVector)
    {
        int remainingBytes = end - offset;
        int unrollableBlocksCount = (remainingBytes / (VECTOR_SIZE * UNROLL_FACTOR)) * UNROLL_FACTOR;
        int remainingBytesBeforeUnrolled = remainingBytes - (unrollableBlocksCount * VECTOR_SIZE);
        int endOffset = offset + remainingBytesBeforeUnrolled;

        while (offset < endOffset)
        {
            Vector128<byte> block = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref start, offset));
            hashVector = AesEncrypt(hashVector, block);
            offset += VECTOR_SIZE;
        }

        return Compress8(ref start, offset, end, hashVector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Compress8(ref byte start, int offset, int end, Vector128<byte> hashVector)
    {
        Vector128<byte> t1 = Vector128<byte>.Zero;
        Vector128<byte> t2 = Vector128<byte>.Zero;
        Vector128<byte> lane1 = hashVector;
        Vector128<byte> lane2 = hashVector;

        ref byte current = ref Unsafe.Add(ref start, offset);
        while (offset < end)
        {
            Vector128<byte> v0 = Unsafe.ReadUnaligned<Vector128<byte>>(ref current);
            Vector128<byte> v1 = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref current, VECTOR_SIZE));
            Vector128<byte> v2 = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref current, (VECTOR_SIZE * 2)));
            Vector128<byte> v3 = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref current, (VECTOR_SIZE * 3)));
            Vector128<byte> v4 = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref current, (VECTOR_SIZE * 4)));
            Vector128<byte> v5 = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref current, (VECTOR_SIZE * 5)));
            Vector128<byte> v6 = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref current, (VECTOR_SIZE * 6)));
            Vector128<byte> v7 = Unsafe.ReadUnaligned<Vector128<byte>>(ref Unsafe.Add(ref current, (VECTOR_SIZE * 7)));

            Vector128<byte> tmp1 = AesEncrypt(v0, v2);
            Vector128<byte> tmp2 = AesEncrypt(v1, v3);

            tmp1 = AesEncrypt(tmp1, v4);
            tmp2 = AesEncrypt(tmp2, v5);

            tmp1 = AesEncrypt(tmp1, v6);
            tmp2 = AesEncrypt(tmp2, v7);

            t1 = Vector128.Add(t1, Key0());
            t2 = Vector128.Add(t2, Key1());

            lane1 = AesEncryptLast(AesEncrypt(tmp1, t1), lane1);
            lane2 = AesEncryptLast(AesEncrypt(tmp2, t2), lane2);

            current = ref Unsafe.Add(ref current, VECTOR_SIZE * UNROLL_FACTOR);
            offset += VECTOR_SIZE * UNROLL_FACTOR;
        }

        Vector128<byte> length = Vector128.Create(end).AsByte();
        lane1 = Vector128.Add(lane1, length);
        lane2 = Vector128.Add(lane2, length);

        return AesEncrypt(lane1, lane2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> GetMaskedVector(ref byte start, int remainingBytes)
    {
        Vector128<sbyte> indices = Vector128.Create(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
        Vector128<byte> mask = Vector128.GreaterThan(Vector128.Create((sbyte)remainingBytes), indices).AsByte();
        Vector128<byte> hashVector = Vector128.BitwiseAnd(mask, Unsafe.ReadUnaligned<Vector128<byte>>(ref start));
        return Vector128.Add(hashVector, Vector128.Create((byte)remainingBytes));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> AesEncrypt(Vector128<byte> value, Vector128<byte> key)
    {
        if (ArmAes.IsSupported)
            return AdvSimd.Xor(ArmAes.MixColumns(ArmAes.Encrypt(value, Vector128<byte>.Zero)), key);

        if (X86Aes.IsSupported)
            return X86Aes.Encrypt(value, key);

        throw new PlatformNotSupportedException(UnsupportedMessage);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> AesEncryptLast(Vector128<byte> value, Vector128<byte> key)
    {
        if (ArmAes.IsSupported)
            return AdvSimd.Xor(ArmAes.Encrypt(value, Vector128<byte>.Zero), key);

        if (X86Aes.IsSupported)
            return X86Aes.EncryptLast(value, key);

        throw new PlatformNotSupportedException(UnsupportedMessage);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Key0() => Vector128.Create(0xF2784542, 0xB09D3E21, 0x89C222E5, 0xFC3BC28E).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Key1() => Vector128.Create(0x03FCE279, 0xCB6B2E9B, 0xB361DC58, 0x39132BD9).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> Key2() => Vector128.Create(0xD0012E32, 0x689D2B7D, 0x5544B1B7, 0xC78B122B).AsByte();


}
#endif