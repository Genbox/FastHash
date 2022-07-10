namespace Genbox.FastHashesNet.FarmHash;

public struct Uint128
{
    public Uint128(ulong low, ulong high)
        : this()
    {
        Low = low;
        High = high;
    }

    public ulong Low { get; set; }

    public ulong High { get; set; }
}