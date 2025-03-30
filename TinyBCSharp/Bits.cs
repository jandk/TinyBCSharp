using System;
using System.Buffers.Binary;

namespace TinyBCSharp
{
    internal class Bits
    {
        private ulong _lo;
        private ulong _hi;

        private Bits(ulong lo, ulong hi)
        {
            _lo = lo;
            _hi = hi;
        }

        internal static Bits From(ReadOnlySpan<byte> array)
        {
            var lo = BinaryPrimitives.ReadUInt64LittleEndian(array);
            var hi = BinaryPrimitives.ReadUInt64LittleEndian(array[8..]);
            return new Bits(lo, hi);
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
}