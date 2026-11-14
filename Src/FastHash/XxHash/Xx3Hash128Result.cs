using System.Runtime.InteropServices;

namespace Genbox.FastHash.XxHash;

// Keep the two hash words internal until the public UInt128 conversion.
[StructLayout(LayoutKind.Sequential)]
internal readonly struct Xx3Hash128Result
{
    internal readonly ulong Low;
    internal readonly ulong High;

    internal Xx3Hash128Result(ulong high, ulong low)
    {
        Low = low;
        High = high;
    }
}