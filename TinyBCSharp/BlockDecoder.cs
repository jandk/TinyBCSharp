using System;

namespace TinyBCSharp
{
    public abstract class BlockDecoder
    {
        private const int BlockWidth = 4;
        private const int BlockHeight = 4;

        private readonly BlockFormat _format;
        internal readonly int _pixelStride;

        internal BlockDecoder(BlockFormat format, int pixelStride)
        {
            _format = format;
            _pixelStride = pixelStride;
        }

        public static BlockDecoder Create(BlockFormat format)
        {
            return format switch
            {
                BlockFormat.BC1 => new BC1Decoder(BC1Mode.Normal),
                BlockFormat.BC1NoAlpha => new BC1Decoder(BC1Mode.Opaque),
                BlockFormat.BC2 => new BC2Decoder(),
                BlockFormat.BC3 => new BC3Decoder(),
                BlockFormat.BC4U => new BC4UDecoder(1),
                BlockFormat.BC4S => new BC4SDecoder(1),
                BlockFormat.BC5U => new BC5UDecoder(false),
                BlockFormat.BC5UReconstructZ => new BC5UDecoder(true),
                BlockFormat.BC5S => new BC5SDecoder(false),
                BlockFormat.BC5SReconstructZ => new BC5SDecoder(true),
                BlockFormat.BC6HUf16 => new BC6HDecoder(false, false),
                BlockFormat.BC6HSf16 => new BC6HDecoder(true, false),
                BlockFormat.BC6HUf32 => new BC6HDecoder(false, true),
                BlockFormat.BC6HSf32 => new BC6HDecoder(true, true),
                BlockFormat.BC7 => new BC7Decoder(),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
        }

        public abstract void DecodeBlock(ReadOnlySpan<byte> src, Span<byte> dst, int stride);

        public byte[] Decode(int width, int height, ReadOnlySpan<byte> src)
        {
            var size = width * height * _pixelStride;
            var dst = new byte[size];
            Decode(width, height, src, dst);
            return dst;
        }

        public void Decode(int width, int height, ReadOnlySpan<byte> src, Span<byte> dst)
        {
            if (width <= 0)
            {
                throw new ArgumentException("width must be greater than 0", nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentException("height must be greater than 0", nameof(height));
            }

            var rowStride = width * _pixelStride;
            if (dst.Length < height * rowStride)
            {
                throw new ArgumentException("dst.Length must be greater than height * bytesPerLine", nameof(dst));
            }

            // Objects.checkFromIndexSize(srcPos, format.size(width, height), src.length);

            var blockStride = BytesPerBlock(_format);

            var srcPos = 0;
            for (var y = 0; y < height; y += BlockHeight)
            {
                var dstPos = y * rowStride;
                for (var x = 0; x < width; x += BlockWidth)
                {
                    var dstOffset = dstPos + x * _pixelStride;
                    if (height - y < 4 || width - x < 4)
                    {
                        PartialBlock(width, height, src[srcPos..], dst[dstOffset..], x, y, rowStride);
                        continue;
                    }

                    DecodeBlock(src[srcPos..], dst[dstOffset..], rowStride);
                    srcPos += blockStride;
                }
            }
        }

        private void PartialBlock(int width, int height, ReadOnlySpan<byte> src, Span<byte> dst, int x, int y,
            int bytesPerLine)
        {
            var blockStride = _pixelStride * BlockWidth;
            var block = (stackalloc byte[BlockHeight * blockStride]);
            DecodeBlock(src, block, blockStride);

            var partialHeight = Math.Min(height - y, BlockHeight);
            var partialStride = Math.Min(width - x, BlockWidth) * _pixelStride;
            for (var yy = 0; yy < partialHeight; yy++)
            {
                block
                    .Slice(yy * blockStride, partialStride)
                    .CopyTo(dst[(yy * bytesPerLine)..]);
            }
        }

        private static int BytesPerBlock(BlockFormat format)
        {
            return format switch
            {
                BlockFormat.BC1 => 8,
                BlockFormat.BC1NoAlpha => 8,
                BlockFormat.BC4U => 8,
                BlockFormat.BC4S => 8,

                BlockFormat.BC2 => 16,
                BlockFormat.BC3 => 16,
                BlockFormat.BC5U => 16,
                BlockFormat.BC5UReconstructZ => 16,
                BlockFormat.BC5S => 16,
                BlockFormat.BC5SReconstructZ => 16,
                BlockFormat.BC6HUf16 => 16,
                BlockFormat.BC6HSf16 => 16,
                BlockFormat.BC6HUf32 => 16,
                BlockFormat.BC6HSf32 => 16,
                BlockFormat.BC7 => 16,

                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
        }
    }
}