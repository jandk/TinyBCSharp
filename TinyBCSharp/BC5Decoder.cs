namespace TinyBCSharp;

abstract class BC5Decoder(BC4Decoder decoder, bool reconstructZ)
    : BlockDecoder(16, BytesPerPixel)
{
    const int BytesPerPixel = 4;

    public override void DecodeBlock(ReadOnlySpan<byte> src, Span<byte> dst, int stride)
    {
        decoder.DecodeBlock(src, dst, stride);
        decoder.DecodeBlock(src[8..], dst[1..], stride);
        WriteAlphas(dst[3..], stride);

        if (reconstructZ)
        {
            ReconstructZ.Reconstruct(dst, stride, BytesPerPixel);
        }
    }

    static void WriteAlphas(Span<byte> dst, int stride)
    {
        for (var y = 0; y < BlockHeight; y++)
        {
            var dstPos = y * stride;
            for (var x = 0; x < BlockWidth; x++)
            {
                dst[dstPos + x * BytesPerPixel] = 0xFF;
            }
        }
    }
}