namespace TinyBCDec;

internal class BC5UDecoder(bool reconstructZ)
    : BlockDecoder(reconstructZ ? BlockFormat.BC5UReconstructZ : BlockFormat.BC5U, BPP)
{
    private const int BPP = 3;

    private readonly BC4UDecoder _decoder = new(BPP);

    public override void DecodeBlock(ReadOnlySpan<byte> src, Span<byte> dst, int stride)
    {
        _decoder.DecodeBlock(src, dst, stride);
        _decoder.DecodeBlock(src[8..], dst[1..], stride);

        if (reconstructZ)
        {
            ReconstructZ.Reconstruct(dst, stride, BPP);
        }
    }
}