namespace TinyBCSharp;

public abstract class BlockDecoder(int bytesPerBlock, int bytesPerPixel)
{
    internal const int BlockWidth = 4;
    internal const int BlockHeight = 4;

    readonly int _bytesPerBlock = bytesPerBlock;
    readonly int _bytesPerPixel = bytesPerPixel;

    public static BlockDecoder Create(BlockFormat format)
    {
        return format switch
        {
            BlockFormat.BC1 => new BC1Decoder(BC1Mode.Normal),
            BlockFormat.BC1NoAlpha => new BC1Decoder(BC1Mode.Opaque),
            BlockFormat.BC2 => new BC2Decoder(),
            BlockFormat.BC3 => new BC3Decoder(),
            BlockFormat.BC4U => new BC4UDecoder(true),
            BlockFormat.BC4S => new BC4SDecoder(true),
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
        var size = width * height * _bytesPerPixel;
        var dst = new byte[size];
        Decode(width, height, src, dst);
        return dst;
    }

    public void Decode(int width, int height, ReadOnlySpan<byte> src, Span<byte> dst)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        var rowStride = width * _bytesPerPixel;
        ArgumentOutOfRangeException.ThrowIfLessThan(dst.Length, height * rowStride);
        // Objects.checkFromIndexSize(srcPos, format.size(width, height), src.length);

        var srcPos = 0;
        for (var y = 0; y < height; y += BlockHeight)
        {
            var dstPos = y * rowStride;
            for (var x = 0; x < width; x += BlockWidth)
            {
                var dstOffset = dstPos + x * _bytesPerPixel;
                if (height >= y + 4 && width >= x + 4)
                    DecodeBlock(src[srcPos..], dst[dstOffset..], rowStride);
                else
                    PartialBlock(width, height, src[srcPos..], dst[dstOffset..], x, y, rowStride);

                srcPos += _bytesPerBlock;
            }
        }
    }

    void PartialBlock(int width, int height, ReadOnlySpan<byte> src, Span<byte> dst, int x, int y,
        int rowStride)
    {
        var blockStride = _bytesPerPixel * BlockWidth;
        var block = (stackalloc byte[BlockHeight * blockStride]);
        DecodeBlock(src, block, blockStride);

        var partialHeight = Math.Min(height - y, BlockHeight);
        var partialStride = Math.Min(width - x, BlockWidth) * _bytesPerPixel;
        for (var yy = 0; yy < partialHeight; yy++)
        {
            block
                .Slice(yy * blockStride, partialStride)
                .CopyTo(dst[(yy * rowStride)..]);
        }
    }
}