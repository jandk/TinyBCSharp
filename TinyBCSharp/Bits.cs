using System.Buffers.Binary;

namespace TinyBCDec;

internal class Bits(long lo, long hi)
{
    private long _lo = lo;
    private long _hi = hi;

    internal static Bits From(ReadOnlySpan<byte> array)
    {
        var lo = BinaryPrimitives.ReadInt64LittleEndian(array);
        var hi = BinaryPrimitives.ReadInt64LittleEndian(array[8..]);
        return new Bits(lo, hi);
    }

    internal long Get64(int count)
    {
        var mask = (1L << count) - 1;
        var bits = _lo & mask;
        _lo = (_lo >>> count) | ((_hi & mask) << (64 - count));
        _hi = (_hi >>> count);
        return bits;
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