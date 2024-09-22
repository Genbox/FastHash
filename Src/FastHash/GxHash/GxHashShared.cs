﻿#if NET8_0
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using ArmAes = System.Runtime.Intrinsics.Arm.Aes;
using X86Aes = System.Runtime.Intrinsics.X86.Aes;

namespace Genbox.FastHash.GxHash;

public static class GxHashShared
{
    private const int VECTOR_SIZE = 16;
    private const int UNROLL_FACTOR = 8;

    // 4096 bytes is a conservative value for the page size
    private const int PAGE_SIZE = 0x1000;

    private static readonly Vector128<byte> _keys1 = Vector128.Create(0xFC3BC28E, 0x89C222E5, 0xB09D3E21, 0xF2784542).AsByte();
    private static readonly Vector128<byte> _keys2 = Vector128.Create(0x03FCE279, 0xCB6B2E9B, 0xB361DC58, 0x39136BD9).AsByte();
    private static readonly Vector128<byte> _keys3 = Vector128.Create(0x713b01d0, 0x8f2f35db, 0xaf163956, 0x85459f85).AsByte();
    private static readonly Vector128<byte> _keys4 = Vector128.Create(0x1de09647, 0x92cfa39c, 0x3dd99aca, 0xb89c054f).AsByte();
    private static readonly Vector128<byte> _keys5 = Vector128.Create(0xc78b122b, 0x5544b1b7, 0x689d2b7d, 0xd0012e32).AsByte();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Vector128<byte> Finalize(Vector128<byte> input, long seed = 0)
    {
        Vector128<byte> output = input;

        if (ArmAes.IsSupported)
        {
            // For some reasons the ARM Neon intrinsics for AES a very different from the ones for X86,
            // so we need these operations below to achieve the same results as for x86
            // See https://blog.michaelbrase.com/2018/05/08/emulating-x86-aes-intrinsics-on-armv8-a
            output = AdvSimd.Xor(ArmAes.MixColumns(ArmAes.Encrypt(output, Vector128<byte>.Zero)), Vector128.Create(seed).AsByte());
            output = AdvSimd.Xor(ArmAes.MixColumns(ArmAes.Encrypt(output, Vector128<byte>.Zero)), _keys3);
            output = AdvSimd.Xor(ArmAes.MixColumns(ArmAes.Encrypt(output, Vector128<byte>.Zero)), _keys4);
            output = AdvSimd.Xor(ArmAes.Encrypt(output, Vector128<byte>.Zero), _keys5);
        }
        else if (X86Aes.IsSupported)
        {
            output = X86Aes.Encrypt(output, Vector128.Create(seed).AsByte());
            output = X86Aes.Encrypt(output, _keys3);
            output = X86Aes.Encrypt(output, _keys4);
            output = X86Aes.EncryptLast(output, _keys5);
        }
        else
            throw new PlatformNotSupportedException("Only works when AES_NI is available");

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
            // Input fits on a single SIMD vector, however we might read beyond the input message
            // Thus we need this safe method that checks if it can safely read beyond or must copy
            return GetPartialVector(ref ptr, len);
        }

        int remainingBytes = len % VECTOR_SIZE;

        // The input does not fit on a single SIMD vector
        Vector128<byte> hashVector;
        if (remainingBytes == 0)
        {
            hashVector = ptr;
            ptr = ref Unsafe.Add(ref ptr, 1);
        }
        else
        {
            // If the input length does not match the length of a whole number of SIMD vectors,
            // it means we'll need to read a partial vector. We can start with the partial vector first,
            // so that we can safely read beyond since we expect the following bytes to still be part of
            // the input
            hashVector = GetPartialVectorUnsafe(ref ptr, remainingBytes);
            ptr = ref Unsafe.AddByteOffset(ref ptr, remainingBytes);
        }

        if (len <= VECTOR_SIZE * 2)
        {
            // Fast path when input length > 16 and <= 32
            hashVector = Compress(hashVector, ptr);
        }
        else if (len <= VECTOR_SIZE * 3)
        {
            // Fast path when input length > 32 and <= 48
            hashVector = Compress(hashVector, Compress(ptr, Unsafe.Add(ref ptr, 1)));
        }
        else
        {
            // Input message is large and we can use the high ILP loop
            hashVector = CompressMany(ref ptr, hashVector, len);
        }

        return hashVector;
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
    private static Vector128<byte> GetPartialVector(ref Vector128<byte> start, int remainingBytes)
    {
        if (IsReadBeyondSafe(ref start))
            return GetPartialVectorUnsafe(ref start, remainingBytes);

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
        if (ArmAes.IsSupported)
        {
            b = AdvSimd.Xor(ArmAes.MixColumns(ArmAes.Encrypt(b, Vector128<byte>.Zero)), _keys1);
            b = AdvSimd.Xor(ArmAes.MixColumns(ArmAes.Encrypt(b, Vector128<byte>.Zero)), _keys2);
            return AdvSimd.Xor(ArmAes.Encrypt(a, Vector128<byte>.Zero), b);
        }
        if (X86Aes.IsSupported)
        {
            b = X86Aes.Encrypt(b, _keys1);
            b = X86Aes.Encrypt(b, _keys2);
            return X86Aes.EncryptLast(a, b);
        }

        throw new PlatformNotSupportedException("Only works when AES_NI is available");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<byte> CompressFast(Vector128<byte> a, Vector128<byte> b)
    {
        if (ArmAes.IsSupported)
            return AdvSimd.Xor(ArmAes.MixColumns(ArmAes.Encrypt(a, Vector128<byte>.Zero)), b);

        if (X86Aes.IsSupported)
            return X86Aes.Encrypt(a, b);

        throw new PlatformNotSupportedException("Only works when AES_NI is available");
    }

    /// <summary>
    /// Returns true if reading the ref value is safe.
    /// This is done using the pointer address and making sure we aren't going to
    /// read past the end of the current memory page (which could produce segfaults)
    /// </summary>
    /// <param name="reference"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe bool IsReadBeyondSafe(ref Vector128<byte> reference)
    {
        IntPtr address = (IntPtr)Unsafe.AsPointer(ref reference);
        IntPtr offsetWithinPage = address & (PAGE_SIZE - 1);
        return offsetWithinPage < PAGE_SIZE - VECTOR_SIZE;
    }
}
#endif