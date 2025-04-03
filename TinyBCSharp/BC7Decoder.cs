using System;
using System.Buffers.Binary;

namespace TinyBCSharp
{
    internal class BC7Decoder : BPTCDecoder
    {
        private const int BytesPerPixel = 4;

        private static readonly Mode[] Modes =
        {
            new Mode(3, 4, F, F, 4, 0, T, F, 3, 0),
            new Mode(2, 6, F, F, 6, 0, F, T, 3, 0),
            new Mode(3, 6, F, F, 5, 0, F, F, 2, 0),
            new Mode(2, 6, F, F, 7, 0, T, F, 2, 0),
            new Mode(1, 0, T, T, 5, 6, F, F, 2, 3),
            new Mode(1, 0, T, F, 7, 8, F, F, 2, 2),
            new Mode(1, 0, F, F, 7, 7, T, F, 4, 0),
            new Mode(2, 6, F, F, 5, 5, T, F, 2, 0)
        };

        public BC7Decoder()
            : base(16, BytesPerPixel)
        {
        }

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

            var partition = mode.pb != 0 ? bits.Get(mode.pb) : 0;
            var rotation = mode.rb ? bits.Get(2) : 0;
            var selection = mode.isb && bits.Get(1) != 0;

            // Great, switching from an int[][] to an int[], increased perf by 40%.
            // I'll take the small readability hit.
            var numColors = mode.ns * 2;
            var colors = (stackalloc int[numColors * 4]);

            // Read colors
            for (var c = 0; c < 3; c++)
            {
                for (var i = 0; i < numColors; i++)
                {
                    colors[i * 4 + c] = bits.Get(mode.cb);
                }
            }

            // Read alphas
            if (mode.ab != 0)
            {
                for (var i = 0; i < numColors; i++)
                {
                    colors[i * 4 + 3] = bits.Get(mode.ab);
                }
            }

            // Read endpoint p-bits
            if (mode.epb)
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
            if (mode.spb)
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
            var extraBits = (mode.epb ? 1 : 0) + (mode.spb ? 1 : 0);
            var colorBits = mode.cb + extraBits;
            var alphaBits = mode.ab + extraBits;
            for (var i = 0; i < numColors; i++)
            {
                if (colorBits < 8)
                {
                    colors[i * 4 + 0] = Unpack(colors[i * 4 + 0], colorBits);
                    colors[i * 4 + 1] = Unpack(colors[i * 4 + 1], colorBits);
                    colors[i * 4 + 2] = Unpack(colors[i * 4 + 2], colorBits);
                }

                if (mode.ab != 0 && alphaBits < 8)
                {
                    colors[i * 4 + 3] = Unpack(colors[i * 4 + 3], alphaBits);
                }
            }

            // Opaque mode
            if (mode.ab == 0)
            {
                for (var i = 0; i < numColors; i++)
                {
                    colors[i * 4 + 3] = 0xFF;
                }
            }

            // Let's try a new method
            var partitions = Partitions[mode.ns][partition];
            var indexBits1 = IndexBits(ref bits, mode.ib1, mode.ns, partition);
            var indexBits2 = IndexBits(ref bits, mode.ib2, mode.ns, partition);
            var weights1 = Weights[mode.ib1];
            var weights2 = Weights[mode.ib2];
            var mask1 = (1 << mode.ib1) - 1;
            var mask2 = (1 << mode.ib2) - 1;

            for (var y = 0; y < BlockHeight; y++)
            {
                var dstPos = y * stride;
                for (var x = 0; x < BlockWidth; x++)
                {
                    int weight1 = weights1[(int)(indexBits1 & mask1)];
                    var cWeight = weight1;
                    var aWeight = weight1;
                    indexBits1 >>= mode.ib1;

                    if (mode.ib2 != 0)
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

                        indexBits2 >>= mode.ib2;
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

        private static int ReadModeIndex(ref Bits bits)
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

        private static void FillInvalidBlock(Span<byte> dst, int stride)
        {
            for (var y = 0; y < BlockHeight; y++)
            {
                dst.Slice(y * stride, BlockWidth * BytesPerPixel).Clear();
            }
        }

        private static int Unpack(int i, int n)
        {
            return i << (8 - n) | i >> (2 * n - 8);
        }

        private struct Mode
        {
            internal byte ns;
            internal byte pb;
            internal bool rb;
            internal bool isb;
            internal byte cb;
            internal byte ab;
            internal bool epb;
            internal bool spb;
            internal byte ib1;
            internal byte ib2;

            internal Mode(byte ns, byte pb, bool rb, bool isb, byte cb, byte ab, bool epb, bool spb, byte ib1, byte ib2)
            {
                this.ns = ns;
                this.pb = pb;
                this.rb = rb;
                this.isb = isb;
                this.cb = cb;
                this.ab = ab;
                this.epb = epb;
                this.spb = spb;
                this.ib1 = ib1;
                this.ib2 = ib2;
            }
        }
    }
}