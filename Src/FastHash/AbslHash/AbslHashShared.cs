// C# port of the Abseil low-level hash from abseil/abseil-cpp (ddbc605). Copyright 2018 The Abseil Authors.
// Distributed under the Apache License 2.0; see THIRD-PARTY-NOTICES.txt at the repository root.
using System.Runtime.CompilerServices;
using static Genbox.FastHash.AbslHash.AbslHashConstants;

namespace Genbox.FastHash.AbslHash;

internal static class AbslHashShared
{
#if NET8_0_OR_GREATER
    internal static bool IsSimdSupported => AbslHashSimd.IsSupported;
#else
    internal static bool IsSimdSupported => false;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong Mix(ulong lhs, ulong rhs) => Fold128To64(lhs, rhs);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ulong CombineRaw(ulong state, ulong value) => Mix(state ^ value, Mul);
}