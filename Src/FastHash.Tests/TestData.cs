namespace Genbox.FastHash.Tests;

internal static class TestData
{
    internal static readonly byte[] CityFarm = CreateCityFarm();
    internal static readonly byte[] XxSanity = CreateXxSanity();

    internal static byte[] Pattern(int length)
    {
        byte[] data = new byte[length];
        for (int i = 0; i < data.Length; i++)
            data[i] = unchecked((byte)((i * 7) + 3));
        return data;
    }

    private static byte[] CreateCityFarm()
    {
        byte[] data = new byte[1 << 20];

        unchecked
        {
            ulong a = 9;
            ulong b = 777;
            const ulong k0 = 0xc3a5c85c97cb3127UL;

            for (int i = 0; i < data.Length; i++)
            {
                a += b;
                b += a;
                a = (a ^ (a >> 41)) * k0;
                b = ((b ^ (b >> 41)) * k0) + (ulong)i;
                data[i] = (byte)(b >> 37);
            }
        }

        return data;
    }

    private static byte[] CreateXxSanity()
    {
        byte[] data = new byte[2367];
        ulong value = 2654435761U;

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(value >> 56);
            value = unchecked(value * 11400714785074694797UL);
        }

        return data;
    }
}