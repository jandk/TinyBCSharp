using System.Runtime.InteropServices;

namespace TinyBCSharp;

struct Bits
{
    ulong _lo, _hi;

    internal static Bits From(ReadOnlySpan<byte> array)
    {
        return MemoryMarshal.Read<Bits>(array);
    }

    internal long Get64(int count)
    {
        var mask = (1UL << count) - 1;
        var bits = _lo & mask;
        _lo = (_lo >> count) | ((_hi & mask) << (64 - count));
        _hi = (_hi >> count);
        return (long)bits;
    }

    internal int Get(int count)
    {
        return (int)Get64(count);
    }

    internal int Get1()
    {
        return Get(1);
    }
}
