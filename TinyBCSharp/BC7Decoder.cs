using System.Buffers.Binary;

namespace TinyBCSharp;

class BC7Decoder() : BPTCDecoder(16, BytesPerPixel)
{
    const int BytesPerPixel = 4;

    static readonly Mode[] Modes =
    [
        new(3, 4, 0, 0, 4, 0, 1, 0, 3, 0),
        new(2, 6, 0, 0, 6, 0, 0, 1, 3, 0),
        new(3, 6, 0, 0, 5, 0, 0, 0, 2, 0),
        new(2, 6, 0, 0, 7, 0, 1, 0, 2, 0),
        new(1, 0, 2, 1, 5, 6, 0, 0, 2, 3),
        new(1, 0, 2, 0, 7, 8, 0, 0, 2, 2),
        new(1, 0, 0, 0, 7, 7, 1, 0, 4, 0),
        new(2, 6, 0, 0, 5, 5, 1, 0, 2, 0)
    ];

    public override void DecodeBlock(ReadOnlySpan<byte> src, Span<byte> dst, int stride)
    {
        var bits = Bits.From(src);
        var modeIndex = ReadModeIndex(ref bits);
        if (modeIndex >= Modes.Length)
        {
            FillInvalidBlock(dst, stride);
            return;
        }

        var mode = Modes[modeIndex];

        var partition = bits.Get(mode.PB);
        var rotation = bits.Get(mode.RB);
        var selection = bits.Get(mode.ISB) != 0;

        // Great, switching from an int[][] to an int[], increased perf by 40%.
        // I'll take the small readability hit.
        var numColors = mode.NS * 2;
        var colors = (stackalloc int[numColors * 4]);

        // Read colors
        for (var c = 0; c < 3; c++)
        {
            for (var i = 0; i < numColors; i++)
            {
                colors[i * 4 + c] = bits.Get(mode.CB);
            }
        }

        // Read alphas
        if (mode.AB != 0)
        {
            for (var i = 0; i < numColors; i++)
            {
                colors[i * 4 + 3] = bits.Get(mode.AB);
            }
        }

        // Read endpoint p-bits
        if (mode.EPB != 0)
        {
            for (var i = 0; i < numColors; i++)
            {
                var pBit = bits.Get1();
                for (var c = 0; c < 4; c++)
                {
                    colors[i * 4 + c] = (colors[i * 4 + c] << 1) | pBit;
                }
            }
        }

        // Read shared p-bits
        if (mode.SPB != 0)
        {
            var sBit1 = bits.Get1();
            var sBit2 = bits.Get1();
            for (var c = 0; c < 4; c++)
            {
                colors[0 * 4 + c] = (colors[0 * 4 + c] << 1) | sBit1;
                colors[1 * 4 + c] = (colors[1 * 4 + c] << 1) | sBit1;
                colors[2 * 4 + c] = (colors[2 * 4 + c] << 1) | sBit2;
                colors[3 * 4 + c] = (colors[3 * 4 + c] << 1) | sBit2;
            }
        }

        // Unpack colors
        var colorBits = mode.CB + (mode.EPB + mode.SPB);
        var alphaBits = mode.AB + (mode.EPB + mode.SPB);
        for (var i = 0; i < numColors; i++)
        {
            if (colorBits < 8)
            {
                colors[i * 4 + 0] = Unpack(colors[i * 4 + 0], colorBits);
                colors[i * 4 + 1] = Unpack(colors[i * 4 + 1], colorBits);
                colors[i * 4 + 2] = Unpack(colors[i * 4 + 2], colorBits);
            }

            if (mode.AB != 0 && alphaBits < 8)
            {
                colors[i * 4 + 3] = Unpack(colors[i * 4 + 3], alphaBits);
            }
        }

        // Opaque mode
        if (mode.AB == 0)
        {
            for (var i = 0; i < numColors; i++)
            {
                colors[i * 4 + 3] = 0xFF;
            }
        }

        // Let's try a new method
        var partitions = Partitions[mode.NS][partition];
        var indexBits1 = IndexBits(ref bits, mode.IB1, mode.NS, partition);
        var indexBits2 = IndexBits(ref bits, mode.IB2, mode.NS, partition);
        var weights1 = Weights[mode.IB1];
        var weights2 = Weights[mode.IB2];
        var mask1 = (1 << mode.IB1) - 1;
        var mask2 = (1 << mode.IB2) - 1;

        for (var y = 0; y < BlockHeight; y++)
        {
            var dstPos = y * stride;
            for (var x = 0; x < BlockWidth; x++)
            {
                int weight1 = weights1[(int)(indexBits1 & mask1)];
                var cWeight = weight1;
                var aWeight = weight1;
                indexBits1 >>= mode.IB1;

                if (mode.IB2 != 0)
                {
                    int weight2 = weights2[(int)(indexBits2 & mask2)];
                    if (selection)
                    {
                        cWeight = weight2;
                    }
                    else
                    {
                        aWeight = weight2;
                    }

                    indexBits2 >>= mode.IB2;
                }

                var pIndex = (int)(partitions & 3);
                var r = Interpolate(colors[pIndex * 8 + 0], colors[pIndex * 8 + 4], cWeight);
                var g = Interpolate(colors[pIndex * 8 + 1], colors[pIndex * 8 + 5], cWeight);
                var b = Interpolate(colors[pIndex * 8 + 2], colors[pIndex * 8 + 6], cWeight);
                var a = Interpolate(colors[pIndex * 8 + 3], colors[pIndex * 8 + 7], aWeight);
                partitions >>= 2;

                if (rotation != 0)
                {
                    switch (rotation)
                    {
                        case 1:
                            (a, r) = (r, a);
                            break;
                        case 2:
                            (a, g) = (g, a);
                            break;
                        case 3:
                            (a, b) = (b, a);
                            break;
                    }
                }

                var index = dstPos + x * BytesPerPixel;
                var color = r | g << 8 | b << 16 | a << 24;
                BinaryPrimitives.WriteInt32LittleEndian(dst[index..], color);
            }
        }
    }

    static int ReadModeIndex(ref Bits bits)
    {
        for (var mode = 0; mode < 8; mode++)
        {
            if (bits.Get1() == 1)
            {
                return mode;
            }
        }

        return 8;
    }

    static void FillInvalidBlock(Span<byte> dst, int stride)
    {
        for (var y = 0; y < BlockHeight; y++)
        {
            dst.Slice(y * stride, BlockWidth * BytesPerPixel).Clear();
        }
    }

    static int Unpack(int i, int n)
    {
        return i << (8 - n) | i >> (2 * n - 8);
    }

    record struct Mode(
        byte NS,
        byte PB,
        byte RB,
        byte ISB,
        byte CB,
        byte AB,
        byte EPB,
        byte SPB,
        byte IB1,
        byte IB2);
}
