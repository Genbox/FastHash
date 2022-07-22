﻿using System.Runtime.InteropServices;

namespace Genbox.FastHash;

[StructLayout(LayoutKind.Sequential)]
public struct Uint128
{
    public Uint128(ulong low, ulong high)
    {
        Low = low;
        High = high;
    }

    public ulong Low;
    public ulong High;
}