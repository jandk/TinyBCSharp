using System.Buffers.Binary;

namespace TinyBCDec;

internal class BC2Decoder() : BlockDecoder(BlockFormat.BC2, BPP)
{
    private const int BPP = 4;

    private readonly BC1Decoder _colorDecoder = new(BC1Mode.BC2Or3);

    public override void DecodeBlock(ReadOnlySpan<byte> src, Span<byte> dst, int stride)
    {
        _colorDecoder.DecodeBlock(src[8..], dst, stride);
        DecodeAlpha(src, dst[3..], stride);
    }

    private static void DecodeAlpha(ReadOnlySpan<byte> src, Span<byte> dst, int stride)
    {
        var alphas = BinaryPrimitives.ReadUInt64LittleEndian(src);
        for (var y = 0; y < 4; y++)
        {
            var dstPos = y * stride;
            for (var x = 0; x < 4; x++)
            {
                dst[dstPos + x * BPP] = (byte)((alphas & 0x0F) * 0x11);
                alphas >>= 4;
            }
        }
    }
}