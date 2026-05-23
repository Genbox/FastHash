#if NET8_0_OR_GREATER
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
        ref Vector128<byte> vectorPtr = ref Unsafe.As<byte, Vector128<byte>>(ref ptr);

        if (len <= VECTOR_SIZE)
        {
            // Input fits on a single SIMD vector, however we might read beyond the input message.
            // Thus we need this safe method that checks if it can safely read beyond or must copy.
            return GetPartialVector(ref vectorPtr, len);
        }

        Vector128<byte> hashVector;
        int offset;

        int extraBytesCount = len % VECTOR_SIZE;
        if (extraBytesCount == 0)
        {
            hashVector = vectorPtr;
            offset = VECTOR_SIZE;
        }
        else
        {
            // If the input length does not match the length of a whole number of SIMD vectors,
            // it means we'll need to read a partial vector. We can start with the partial vector first,
            // so that we can safely read beyond since we expect the following bytes to still be part of
            // the input.
            hashVector = GetPartialVectorUnsafe(ref vectorPtr, extraBytesCount);
            offset = extraBytesCount;
        }

        Vector128<byte> tail = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref ptr, offset));
        offset += VECTOR_SIZE;

        if (len > VECTOR_SIZE * 2)
        {
            // Fast path when input length > 32 and <= 48
            Vector128<byte> next = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref ptr, offset));
            offset += VECTOR_SIZE;
            tail = AesEncrypt(tail, next);

            if (len > VECTOR_SIZE * 3)
            {
                // Fast path when input length > 48 and <= 64
                next = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref ptr, offset));
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
            Vector128<byte> block = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref start, offset));
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

        while (offset < end)
        {
            Vector128<byte> v0 = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref start, offset));
            Vector128<byte> v1 = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref start, offset + VECTOR_SIZE));
            Vector128<byte> v2 = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref start, offset + (VECTOR_SIZE * 2)));
            Vector128<byte> v3 = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref start, offset + (VECTOR_SIZE * 3)));
            Vector128<byte> v4 = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref start, offset + (VECTOR_SIZE * 4)));
            Vector128<byte> v5 = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref start, offset + (VECTOR_SIZE * 5)));
            Vector128<byte> v6 = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref start, offset + (VECTOR_SIZE * 6)));
            Vector128<byte> v7 = Unsafe.As<byte, Vector128<byte>>(ref Unsafe.Add(ref start, offset + (VECTOR_SIZE * 7)));

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

            offset += VECTOR_SIZE * UNROLL_FACTOR;
        }

        Vector128<byte> length = Vector128.Create(end).AsByte();
        lane1 = Vector128.Add(lane1, length);
        lane2 = Vector128.Add(lane2, length);

        return AesEncrypt(lane1, lane2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Vector128<byte> GetPartialVector(ref Vector128<byte> start, int remainingBytes)
    {
        fixed (Vector128<byte>* _ = &start)
        {
            if (IsReadBeyondSafe(ref start))
                return GetPartialVectorUnsafe(ref start, remainingBytes);
        }

        return GetPartialVectorSafe(ref start, remainingBytes);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> GetPartialVectorSafe(ref Vector128<byte> start, int remainingBytes)
    {
        Vector128<byte> input = Vector128<byte>.Zero;
        ref byte source = ref Unsafe.As<Vector128<byte>, byte>(ref start);
        ref byte dest = ref Unsafe.As<Vector128<byte>, byte>(ref input);
        Unsafe.CopyBlockUnaligned(ref dest, ref source, (uint)remainingBytes);
        return Vector128.Add(input, Vector128.Create((byte)remainingBytes));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> GetPartialVectorUnsafe(ref Vector128<byte> start, int remainingBytes)
    {
        Vector128<sbyte> indices = Vector128.Create(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
        Vector128<byte> mask = Vector128.GreaterThan(Vector128.Create((sbyte)remainingBytes), indices).AsByte();
        Vector128<byte> hashVector = Vector128.BitwiseAnd(mask, start);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe bool IsReadBeyondSafe(ref Vector128<byte> reference)
    {
        // 4096 bytes is a conservative value for the page size.
        const int PAGE_SIZE = 0x1000;
        IntPtr address = (IntPtr)Unsafe.AsPointer(ref reference);
        IntPtr offsetWithinPage = address & (PAGE_SIZE - 1);
        return offsetWithinPage < PAGE_SIZE - VECTOR_SIZE;
    }
}
#endif