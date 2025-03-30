namespace TinyBCDec;

internal class BC3Decoder() : BlockDecoder(BlockFormat.BC3, 4)
{
    private const int BPP = 4;

    private readonly BC1Decoder _colorDecoder = new(BC1Mode.BC2Or3);
    private readonly BC4UDecoder _alphaDecoder = new(BPP);

    public override void DecodeBlock(ReadOnlySpan<byte> src, Span<byte> dst, int stride)
    {
        _colorDecoder.DecodeBlock(src[8..], dst, stride);
        _alphaDecoder.DecodeBlock(src, dst[3..], stride);
    }
}