namespace TinyBCDec;

public abstract class BlockDecoder
{
    private const int BlockWidth = 4;
    private const int BlockHeight = 4;

    private readonly BlockFormat _format;
    private readonly int _pixelStride;

    internal BlockDecoder(BlockFormat format, int pixelStride)
    {
        _format = format;
        _pixelStride = pixelStride;
    }

    public static BlockDecoder Create(BlockFormat format)
    {
        switch (format)
        {
            case BlockFormat.BC1:
                return new BC1Decoder(BC1Mode.Normal);
            case BlockFormat.BC1NoAlpha:
                return new BC1Decoder(BC1Mode.Opaque);
            case BlockFormat.BC2:
                return new BC2Decoder();
            case BlockFormat.BC3:
                return new BC3Decoder();
            case BlockFormat.BC4U:
                return new BC4UDecoder(1);
            case BlockFormat.BC4S:
                return new BC4SDecoder(1);
            case BlockFormat.BC5U:
                return new BC5UDecoder(false);
            case BlockFormat.BC5UReconstructZ:
                return new BC5UDecoder(true);
            case BlockFormat.BC5S:
                return new BC5SDecoder(false);
            case BlockFormat.BC5SReconstructZ:
                return new BC5SDecoder(true);
            case BlockFormat.BC6HUf16:
                break;
            case BlockFormat.BC6HSf16:
                break;
            case BlockFormat.BC6HUf32:
                break;
            case BlockFormat.BC6HSf32:
                break;
            case BlockFormat.BC7:
                return new BC7Decoder();
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        throw new ArgumentOutOfRangeException(nameof(format), format, null);
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

        var bytesPerRow = width * _pixelStride;
        if (dst.Length < height * bytesPerRow)
        {
            throw new ArgumentException("dst.Length must be greater than height * bytesPerLine", nameof(dst));
        }

        // Objects.checkFromIndexSize(srcPos, format.size(width, height), src.length);

        var bytesPerBlock = BytesPerBlock(_format);

        var srcPos = 0;
        for (var y = 0; y < height; y += BlockHeight)
        {
            var dstPos = y * bytesPerRow;
            for (var x = 0; x < width; x += BlockWidth)
            {
                var dstOffset = dstPos + x * _pixelStride;
                if (height - y < 4 || width - x < 4)
                {
                    PartialBlock(width, height, src[srcPos..], dst[dstOffset..], x, y, bytesPerRow);
                    continue;
                }

                DecodeBlock(src[srcPos..], dst[dstOffset..], bytesPerRow);
                srcPos += bytesPerBlock;
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
            BlockFormat.BC1 or
                BlockFormat.BC1NoAlpha or
                BlockFormat.BC4U or
                BlockFormat.BC4S => 8,

            BlockFormat.BC2 or
                BlockFormat.BC3 or
                BlockFormat.BC5U or
                BlockFormat.BC5UReconstructZ or
                BlockFormat.BC5S or
                BlockFormat.BC5SReconstructZ or
                BlockFormat.BC6HUf16 or
                BlockFormat.BC6HSf16 or
                BlockFormat.BC6HUf32 or
                BlockFormat.BC6HSf32 or
                BlockFormat.BC7 => 16,

            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}