using System.Buffers.Binary;

namespace TinyBCDec;

internal class BC6HDecoder(bool signed, bool asSingle)
    : BPTCDecoder(signed ? BlockFormat.BC6HSf16 : BlockFormat.BC6HUf16, asSingle ? 12 : 6)
{
    // @formatter:off
    private static readonly Mode[] Modes =
    [
        new(true, +5, 10, +5, +5, +5, [ 0x0741, 0x0841, 0x0B41, 0x000A, 0x010A, 0x020A, 0x0305, 0x0A41, 0x0704, 0x0405, 0x0B01, 0x0A04, 0x0505, 0x0B11, 0x0804, 0x0605, 0x0B21, 0x0905, 0x0B31 ]),
        new(true, +5, +7, +6, +6, +6, [ 0x0751, 0x0A41, 0x0A51, 0x0007, 0x0B01, 0x0B11, 0x0841, 0x0107, 0x0851, 0x0B21, 0x0741, 0x0207, 0x0B31, 0x0B51, 0x0B41, 0x0306, 0x0704, 0x0406, 0x0A04, 0x0506, 0x0804, 0x0606, 0x0906 ]),
        new(true, +5, 11, +5, +4, +4, [ 0x000A, 0x010A, 0x020A, 0x0305, 0x00A1, 0x0704, 0x0404, 0x01A1, 0x0B01, 0x0A04, 0x0504, 0x02A1, 0x0B11, 0x0804, 0x0605, 0x0B21, 0x0905, 0x0B31 ]),
        new(true, +5, 11, +4, +5, +4, [ 0x000A, 0x010A, 0x020A, 0x0304, 0x00A1, 0x0A41, 0x0704, 0x0405, 0x01A1, 0x0A04, 0x0504, 0x02A1, 0x0B11, 0x0804, 0x0604, 0x0B01, 0x0B21, 0x0904, 0x0741, 0x0B31 ]),
        new(true, +5, 11, +4, +4, +5, [ 0x000A, 0x010A, 0x020A, 0x0304, 0x00A1, 0x0841, 0x0704, 0x0404, 0x01A1, 0x0B01, 0x0A04, 0x0505, 0x02A1, 0x0804, 0x0604, 0x0B11, 0x0B21, 0x0904, 0x0B41, 0x0B31 ]),
        new(true, +5, +9, +5, +5, +5, [ 0x0009, 0x0841, 0x0109, 0x0741, 0x0209, 0x0B41, 0x0305, 0x0A41, 0x0704, 0x0405, 0x0B01, 0x0A04, 0x0505, 0x0B11, 0x0804, 0x0605, 0x0B21, 0x0905, 0x0B31 ]),
        new(true, +5, +8, +6, +5, +5, [ 0x0008, 0x0A41, 0x0841, 0x0108, 0x0B21, 0x0741, 0x0208, 0x0B31, 0x0B41, 0x0306, 0x0704, 0x0405, 0x0B01, 0x0A04, 0x0505, 0x0B11, 0x0804, 0x0606, 0x0906 ]),
        new(true, +5, +8, +5, +6, +5, [ 0x0008, 0x0B01, 0x0841, 0x0108, 0x0751, 0x0741, 0x0208, 0x0A51, 0x0B41, 0x0305, 0x0A41, 0x0704, 0x0406, 0x0A04, 0x0505, 0x0B11, 0x0804, 0x0605, 0x0B21, 0x0905, 0x0B31 ]),
        new(true, +5, +8, +5, +5, +6, [ 0x0008, 0x0B11, 0x0841, 0x0108, 0x0851, 0x0741, 0x0208, 0x0B51, 0x0B41, 0x0305, 0x0A41, 0x0704, 0x0405, 0x0B01, 0x0A04, 0x0506, 0x0804, 0x0605, 0x0B21, 0x0905, 0x0B31 ]),
        new(false, 5, +6, +6, +6, +6, [ 0x0006, 0x0A41, 0x0B01, 0x0B11, 0x0841, 0x0106, 0x0751, 0x0851, 0x0B21, 0x0741, 0x0206, 0x0A51, 0x0B31, 0x0B51, 0x0B41, 0x0306, 0x0704, 0x0406, 0x0A04, 0x0506, 0x0804, 0x0606, 0x0906 ]),
        new(false, 0, 10, 10, 10, 10, [ 0x000A, 0x010A, 0x020A, 0x030A, 0x040A, 0x050A ]),
        new(true, +0, 11, +9, +9, +9, [ 0x000A, 0x010A, 0x020A, 0x0309, 0x00A1, 0x0409, 0x01A1, 0x0509, 0x02A1 ]),
        new(true, +0, 12, +8, +8, +8, [ 0x000A, 0x010A, 0x020A, 0x0308, 0x10A2, 0x0408, 0x11A2, 0x0508, 0x12A2 ]),
        new(true, +0, 16, +4, +4, +4, [ 0x000A, 0x010A, 0x020A, 0x0304, 0x10A6, 0x0404, 0x11A6, 0x0504, 0x12A6 ]),
    ];
    // @formatter:on

    private static ReadOnlySpan<byte> Reverse2 => [0, 2, 1, 3];

    private static ReadOnlySpan<byte> Reverse6 =>
    [
        0, 32, 16, 48, +8, 40, 24, 56, 4, 36, 20, 52, 12, 44, 28, 60,
        2, 34, 18, 50, 10, 42, 26, 58, 6, 38, 22, 54, 14, 46, 30, 62,
        1, 33, 17, 49, +9, 41, 25, 57, 5, 37, 21, 53, 13, 45, 29, 61,
        3, 35, 19, 51, 11, 43, 27, 59, 7, 39, 23, 55, 15, 47, 31, 63
    ];

    public override void DecodeBlock(ReadOnlySpan<byte> src, Span<byte> dst, int stride)
    {
        var bits = Bits.From(src);
        var modeIndex = ModeIndex(bits);
        if (modeIndex >= Modes.Length)
        {
            FillInvalidBlock(dst, stride);
            return;
        }

        var mode = Modes[modeIndex];
        var colors = (stackalloc int[12]);
        foreach (var op in mode.ops)
        {
            ReadOp(bits, op, colors);
        }

        var partition = bits.Get(mode.pb);
        var numPartitions = mode.pb == 0 ? 1 : 2;

        if (signed)
        {
            colors[0] = ExtendSign(colors[0], mode.epb);
            colors[1] = ExtendSign(colors[1], mode.epb);
            colors[2] = ExtendSign(colors[2], mode.epb);
        }

        if (mode.te || signed)
        {
            for (var i = 3; i < numPartitions * 6; i += 3)
            {
                colors[i + 0] = ExtendSign(colors[i + 0], mode.rb);
                colors[i + 1] = ExtendSign(colors[i + 1], mode.gb);
                colors[i + 2] = ExtendSign(colors[i + 2], mode.bb);
            }
        }

        if (mode.te)
        {
            for (var i = 3; i < numPartitions * 6; i += 3)
            {
                colors[i + 0] = TransformInverse(colors[i + 0], colors[0], mode.epb, signed);
                colors[i + 1] = TransformInverse(colors[i + 1], colors[1], mode.epb, signed);
                colors[i + 2] = TransformInverse(colors[i + 2], colors[2], mode.epb, signed);
            }
        }

        for (var i = 0; i < numPartitions * 6; i += 3)
        {
            colors[i + 0] = Unquantize(colors[i + 0], mode.epb, signed);
            colors[i + 1] = Unquantize(colors[i + 1], mode.epb, signed);
            colors[i + 2] = Unquantize(colors[i + 2], mode.epb, signed);
        }

        var ib = numPartitions == 1 ? 4 : 3;
        var partitions = Partitions[numPartitions][partition];
        var indexBits = IndexBits(bits, ib, numPartitions, partition);
        var weights = Weights[ib];
        var mask = (1 << ib) - 1;
        for (var y = 0; y < 4; y++)
        {
            var dstPos = y * stride;
            for (var x = 0; x < 4; x++)
            {
                int weight = weights[(int)(indexBits & mask)];
                indexBits >>= ib;

                var pIndex = (int)(partitions & 3);
                var r = FinalUnquantize(Interpolate(colors[pIndex * 6 + 0], colors[pIndex * 6 + 3], weight), signed);
                var g = FinalUnquantize(Interpolate(colors[pIndex * 6 + 1], colors[pIndex * 6 + 4], weight), signed);
                var b = FinalUnquantize(Interpolate(colors[pIndex * 6 + 2], colors[pIndex * 6 + 5], weight), signed);
                partitions >>= 2;

                if (asSingle)
                    WritePixelF32(dst[(dstPos + x * _pixelStride)..], r, g, b);
                else
                    WritePixelF16(dst[(dstPos + x * _pixelStride)..], r, g, b);
            }
        }
    }

    private static int ModeIndex(Bits bits)
    {
        var mode = bits.Get(2);
        return mode < 2 ? mode : bits.Get(3) + (mode * 8 - 14);
    }

    private static void ReadOp(Bits bits, short op, Span<int> colors)
    {
        // count | shift << 4 | index << 8 | (reverse ? 1 : 0) << 12
        var count = op & 0x0F;
        var shift = (op >> 4) & 0x0F;
        var index = (op >> 8) & 0x0F;
        var reverse = (op >> 12) != 0;

        var value = bits.Get(count);
        if (reverse)
        {
            value = count switch
            {
                2 => Reverse2[value],
                6 => Reverse6[value],
                _ => throw new NotImplementedException()
            };
        }

        colors[index] |= value << shift;
    }

    private static int ExtendSign(int value, int bits)
    {
        var signBit = 1 << (bits - 1);
        return (value ^ signBit) - signBit;
    }

    private static int TransformInverse(int value, int value0, int bits, bool signed)
    {
        value = (value + value0) & ((1 << bits) - 1);
        return signed ? ExtendSign(value, bits) : value;
    }

    private static int Unquantize(int value, int bits, bool signed)
    {
        return signed
            ? UnquantizeSigned(value, bits)
            : UnquantizeUnsigned(value, bits);
    }

    private static int UnquantizeSigned(int value, int bits)
    {
        if (bits >= 16 || value == 0)
            return value;

        var sign = value < 0;
        value = Math.Abs(value);

        int unq;
        if (value >= (1 << (bits - 1)) - 1)
            unq = 0x7FFF;
        else
            unq = ((value << 15) + 0x4000) >> (bits - 1);

        return sign ? -unq : unq;
    }

    private static int UnquantizeUnsigned(int value, int bits)
    {
        if (bits >= 15 || value == 0)
            return value;

        if (value == ((1 << bits) - 1))
            return 0xFFFF;

        return ((value << 15) + 0x4000) >> (bits - 1);
    }

    private static short FinalUnquantize(int i, bool signed)
    {
        if (signed)
        {
            i = (short)i;
            return (short)(i < 0 ? (((-i) * 31) >> 5) + 0x8000 : (i * 31) >> 5);
        }
        else
        {
            return (short)((i * 31) >> 6);
        }
    }

    private static void WritePixelF16(Span<byte> dst, short r, short g, short b)
    {
        BinaryPrimitives.WriteInt16LittleEndian(dst, r);
        BinaryPrimitives.WriteInt16LittleEndian(dst[2..], g);
        BinaryPrimitives.WriteInt16LittleEndian(dst[4..], b);
    }

    private static void WritePixelF32(Span<byte> dst, short r, short g, short b)
    {
        BinaryPrimitives.WriteSingleLittleEndian(dst, (float)BitConverter.Int16BitsToHalf(r));
        BinaryPrimitives.WriteSingleLittleEndian(dst[4..], (float)BitConverter.Int16BitsToHalf(g));
        BinaryPrimitives.WriteSingleLittleEndian(dst[8..], (float)BitConverter.Int16BitsToHalf(b));
    }

    private record Mode(
        bool te,
        byte pb,
        byte epb,
        byte rb,
        byte gb,
        byte bb,
        short[] ops
    );
}