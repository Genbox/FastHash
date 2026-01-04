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
    private const int VECTOR_SIZE = 16;
    private const int UNROLL_FACTOR = 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> Finalize(Vector128<byte> input)
    {
        Vector128<byte> keys1 = Vector128.Create(0x713b01d0, 0x8f2f35db, 0xaf163956, 0x85459f85).AsByte();
        Vector128<byte> keys2 = Vector128.Create(0x1de09647, 0x92cfa39c, 0x3dd99aca, 0xb89c054f).AsByte();
        Vector128<byte> keys3 = Vector128.Create(0xc78b122b, 0x5544b1b7, 0x689d2b7d, 0xd0012e32).AsByte();

        Vector128<byte> output = input;

        if (ArmAes.IsSupported)
        {
            // ARM AES intrinsics require this sequence to match x86 AES output.
            output = AdvSimd.Xor(ArmAes.MixColumns(ArmAes.Encrypt(output, Vector128<byte>.Zero)), keys1);
            output = AdvSimd.Xor(ArmAes.MixColumns(ArmAes.Encrypt(output, Vector128<byte>.Zero)), keys2);
            output = AdvSimd.Xor(ArmAes.Encrypt(output, Vector128<byte>.Zero), keys3);
        }
        else if (X86Aes.IsSupported)
        {
            output = X86Aes.Encrypt(output, keys1);
            output = X86Aes.Encrypt(output, keys2);
            output = X86Aes.EncryptLast(output, keys3);
        }
        else
        {
            throw new PlatformNotSupportedException("Only works when AES_NI is available");
        }

        return output;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> Compress(ReadOnlySpan<byte> bytes)
    {
        // Get pointer of SIMD vectors from input span
        ref Vector128<byte> ptr = ref Unsafe.As<byte, Vector128<byte>>(ref MemoryMarshal.GetReference(bytes));

        int len = bytes.Length;

        if (len <= VECTOR_SIZE)
        {
            // Input fits on a single SIMD vector, however we might read beyond the input message.
            return GetPartialVector(ref ptr, len);
        }

        Vector128<byte> hashVector;
        int remainingBytes;

        int extraBytesCount = len % VECTOR_SIZE;
        if (extraBytesCount == 0)
        {
            hashVector = ptr;
            ptr = ref Unsafe.Add(ref ptr, 1);
            remainingBytes = len - VECTOR_SIZE;
        }
        else
        {
            // Start with partial vector so we can safely read beyond later.
            hashVector = GetPartialVectorUnsafe(ref ptr, extraBytesCount);
            ptr = ref Unsafe.AddByteOffset(ref ptr, extraBytesCount);
            remainingBytes = len - extraBytesCount;
        }

        if (len <= VECTOR_SIZE * 2)
        {
            hashVector = Compress(hashVector, ptr);
        }
        else if (len <= VECTOR_SIZE * 3)
        {
            hashVector = Compress(hashVector, Compress(ptr, Unsafe.Add(ref ptr, 1)));
        }
        else
        {
            hashVector = CompressMany(ref ptr, hashVector, remainingBytes);
        }

        return hashVector;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> CompressFast(Vector128<byte> a, Vector128<byte> b)
    {
        if (ArmAes.IsSupported)
            return AdvSimd.Xor(ArmAes.MixColumns(ArmAes.Encrypt(a, Vector128<byte>.Zero)), b);

        if (X86Aes.IsSupported)
            return X86Aes.Encrypt(a, b);

        throw new PlatformNotSupportedException("Only works when AES_NI is available");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> CompressMany(ref Vector128<byte> start, Vector128<byte> hashVector, int len)
    {
        int unrollableBlocksCount = len / (VECTOR_SIZE * UNROLL_FACTOR) * UNROLL_FACTOR;
        ref Vector128<byte> end2 = ref Unsafe.Add(ref start, unrollableBlocksCount);

        while (Unsafe.IsAddressLessThan(ref start, ref end2))
        {
            Vector128<byte> blockHash = start;
            blockHash = CompressFast(blockHash, Unsafe.Add(ref start, 1));
            blockHash = CompressFast(blockHash, Unsafe.Add(ref start, 2));
            blockHash = CompressFast(blockHash, Unsafe.Add(ref start, 3));
            blockHash = CompressFast(blockHash, Unsafe.Add(ref start, 4));
            blockHash = CompressFast(blockHash, Unsafe.Add(ref start, 5));
            blockHash = CompressFast(blockHash, Unsafe.Add(ref start, 6));
            blockHash = CompressFast(blockHash, Unsafe.Add(ref start, 7));
            start = ref Unsafe.Add(ref start, UNROLL_FACTOR);

            hashVector = Compress(hashVector, blockHash);
        }

        int remainingBlocksCount = (len / VECTOR_SIZE) - unrollableBlocksCount;

        ref Vector128<byte> end = ref Unsafe.Add(ref start, remainingBlocksCount);

        while (Unsafe.IsAddressLessThan(ref start, ref end))
        {
            hashVector = Compress(hashVector, start);
            start = ref Unsafe.Add(ref start, 1);
        }

        return hashVector;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe Vector128<byte> GetPartialVector(ref Vector128<byte> start, int remainingBytes)
    {
        fixed (Vector128<byte>* pin = &start)
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
    private static Vector128<byte> Compress(Vector128<byte> a, Vector128<byte> b)
    {
        Vector128<byte> keys1 = Vector128.Create(0xFC3BC28E, 0x89C222E5, 0xB09D3E21, 0xF2784542).AsByte();
        Vector128<byte> keys2 = Vector128.Create(0x03FCE279, 0xCB6B2E9B, 0xB361DC58, 0x39136BD9).AsByte();

        if (ArmAes.IsSupported)
        {
            b = AdvSimd.Xor(ArmAes.MixColumns(ArmAes.Encrypt(b, Vector128<byte>.Zero)), keys1);
            b = AdvSimd.Xor(ArmAes.MixColumns(ArmAes.Encrypt(b, Vector128<byte>.Zero)), keys2);
            return AdvSimd.Xor(ArmAes.Encrypt(a, Vector128<byte>.Zero), b);
        }
        if (X86Aes.IsSupported)
        {
            b = X86Aes.Encrypt(b, keys1);
            b = X86Aes.Encrypt(b, keys2);
            return X86Aes.EncryptLast(a, b);
        }

        throw new PlatformNotSupportedException("Only works when AES_NI is available");
    }

    /// <summary>
    /// Returns true if reading the ref value is safe.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe bool IsReadBeyondSafe(ref Vector128<byte> reference)
    {
        // 4096 bytes is a conservative value for the page size.
        const int pageSize = 0x1000;
        IntPtr address = (IntPtr)Unsafe.AsPointer(ref reference);
        IntPtr offsetWithinPage = address & (pageSize - 1);
        return offsetWithinPage < pageSize - VECTOR_SIZE;
    }
}
#endif
