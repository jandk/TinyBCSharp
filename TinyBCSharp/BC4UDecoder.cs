using System;
using System.Buffers.Binary;

namespace TinyBCSharp
{
    internal class BC4UDecoder : BlockDecoder
    {
        public BC4UDecoder(int pixelStride)
            : base(BlockFormat.BC4U, pixelStride)
        {
        }

        public override void DecodeBlock(ReadOnlySpan<byte> src, Span<byte> dst, int stride)
        {
            var block = BinaryPrimitives.ReadUInt64LittleEndian(src);

            // @formatter:off
            var a0 = (uint)  block       & 0xFF;
            var a1 = (uint) (block >> 8) & 0xFF;

            var alphas = (stackalloc byte[8]);
            alphas[0] = (byte)a0;
            alphas[1] = (byte)a1;
            alphas[7] = 0xFF;
            
            if (a0 > a1) {
                alphas[2] = Scale1785(6 * a0 +     a1);
                alphas[3] = Scale1785(5 * a0 + 2 * a1);
                alphas[4] = Scale1785(4 * a0 + 3 * a1);
                alphas[5] = Scale1785(3 * a0 + 4 * a1);
                alphas[6] = Scale1785(2 * a0 + 5 * a1);
                alphas[7] = Scale1785(    a0 + 6 * a1);
            } else {
                alphas[2] = Scale1275(4 * a0 +     a1);
                alphas[3] = Scale1275(3 * a0 + 2 * a1);
                alphas[4] = Scale1275(2 * a0 + 3 * a1);
                alphas[5] = Scale1275(    a0 + 4 * a1);
            }
            // @formatter:on

            var indices = block >> 16;
            for (var y = 0; y < 4; y++)
            {
                var dstPos = y * stride;
                for (var x = 0; x < 4; x++)
                {
                    var alpha = alphas[(int)(indices & 7)];
                    dst[dstPos + x * _pixelStride] = alpha;
                    indices >>= 3;
                }
            }
        }

        private static byte Scale1785(uint i) => (byte)((i * 585 + 2010) >> 12);
        private static byte Scale1275(uint i) => (byte)((i * 819 + 1893) >> 12);
    }
}