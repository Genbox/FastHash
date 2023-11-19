using Genbox.FastHash.T1haHash;

namespace Genbox.FastHash.Examples;

public static class Program
{
    public static unsafe void Main()
    {
        //Nothing for now
        byte[] data = "hello world"u8.ToArray();

        fixed (byte* ptr = data)
        {
            ulong res = T1ha0Hash64.ComputeHash(ptr, (uint)data.Length);
            Console.WriteLine(res);
        }
    }
}